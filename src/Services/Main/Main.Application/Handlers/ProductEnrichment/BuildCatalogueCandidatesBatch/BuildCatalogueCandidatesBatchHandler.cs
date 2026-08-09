using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Product.Enrichment;

namespace Main.Application.Handlers.ProductEnrichment.BuildCatalogueCandidatesBatch;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2, "23505")]
public sealed record BuildCatalogueCandidatesBatchCommand(
    int LastProcessedId,
    int BatchSize)
    : ICommand<BuildCatalogueCandidatesBatchResult>;

public sealed record BuildCatalogueCandidatesBatchResult(
    int LastProcessedId,
    int ReadRows,
    int AssignedRows,
    int SkippedRows,
    bool HasMore);

public sealed class BuildCatalogueCandidatesBatchHandler(
    ISupplierProductRepository supplierProductRepository,
    IRepository<CatalogueCandidate, int> catalogueCandidateRepository,
    IProducerLookupService producerLookupService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<BuildCatalogueCandidatesBatchCommand, BuildCatalogueCandidatesBatchResult>
{
    public async Task<BuildCatalogueCandidatesBatchResult> Handle(
        BuildCatalogueCandidatesBatchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(request.LastProcessedId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.BatchSize);

        var supplierProducts = await supplierProductRepository.ListAsync(
            Criteria<SupplierProduct>
                .New()
                .Where(x => x.CatalogueCandidateId == null)
                .Where(x => x.Id > request.LastProcessedId)
                .OrderByAsc(x => x.Id)
                .Track()
                .Size(request.BatchSize)
                .Build(),
            cancellationToken);

        if (supplierProducts.Count == 0)
            return new BuildCatalogueCandidatesBatchResult(
                request.LastProcessedId,
                0,
                0,
                0,
                false);

        var producerLookup = await producerLookupService.Load(cancellationToken);
        var resolvedProducts = supplierProducts
            .Select(x => new ResolvedSupplierProduct(
                x,
                producerLookup.ResolveId(x.Producer, x.Supplier)))
            .Where(x => x.ProducerId.HasValue)
            .ToList();

        var candidateGroups = resolvedProducts
            .GroupBy(x => (
                x.Product.Sku.NormalizedValue,
                ProducerId: x.ProducerId!.Value))
            .ToDictionary(
                x => x.Key, 
                x => x.ToList());

        var normalizedSkus = candidateGroups.Keys
            .Select(x => x.NormalizedValue)
            .Distinct()
            .ToList();
        var producerIds = candidateGroups.Keys
            .Select(x => x.ProducerId)
            .Distinct()
            .ToList();

        var persistedCandidates = (await catalogueCandidateRepository.ListAsync(
                Criteria<CatalogueCandidate>
                    .New()
                    .Where(x => normalizedSkus.Contains(x.Sku.NormalizedValue))
                    .Where(x => producerIds.Contains(x.ProducerId))
                    .Track()
                    .Build(),
                cancellationToken))
            .Where(x => candidateGroups.ContainsKey((
                x.Sku.NormalizedValue,
                x.ProducerId)))
            .ToDictionary(
                x => (x.Sku.NormalizedValue, x.ProducerId));

        var missingCandidates = candidateGroups
            .Where(x => !persistedCandidates.ContainsKey(x.Key))
            .Select(x => CatalogueCandidate.Create(
                x.Value[0].Product.Sku.Value,
                x.Key.ProducerId))
            .ToList();

        if (missingCandidates.Count > 0)
        {
            await unitOfWork.AddRangeAsync(
                missingCandidates,
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var candidate in missingCandidates)
                persistedCandidates.Add(
                    (candidate.Sku.NormalizedValue, candidate.ProducerId),
                    candidate);
        }

        foreach (var resolved in resolvedProducts)
        {
            var key = (
                resolved.Product.Sku.NormalizedValue,
                resolved.ProducerId!.Value);
            resolved.Product.AssignToCatalogueCandidate(
                persistedCandidates[key].Id);
        }

        return new BuildCatalogueCandidatesBatchResult(
            supplierProducts[^1].Id,
            supplierProducts.Count,
            resolvedProducts.Count,
            supplierProducts.Count - resolvedProducts.Count,
            supplierProducts.Count == request.BatchSize);
    }

    private sealed record ResolvedSupplierProduct(
        SupplierProduct Product,
        int? ProducerId);
}
