using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using MassTransit;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Product.Enrichment;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.BuildCatalogueCandidates;

public class BuildCatalogueCandidatesLrt(
    IRepository<Job, Guid> jobRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    ISupplierProductRepository supplierProductRepository,
    IRepository<CatalogueCandidate, Guid> catalogueCandidateRepository,
    IProducerLookupService producerLookupService,
    ILogger<BuildCatalogueCandidatesLrt> logger
    ) : LrtBase<NoneInputState, BuildCatalogueCandidatesState>(
    jobRepository,
    unitOfWork,
    publisher,
    transactionService,
    logger)
{
    private static readonly TransactionalAttribute BatchTransactionSettings =
        new(IsolationLevel.ReadCommitted, 20, 2, "23505");

    public const string LrtSystemName = nameof(BuildCatalogueCandidatesLrt);
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey => "lrt.catalogue.candidates.build.name";
    public override string DescriptionLocalizationKey => "lrt.catalogue.candidates.build.description";
    protected override async Task DoWork()
    {
        const int batchSize = 1000;
        while (true)
        {
            var result = await ProcessBatchAsync(
                State.LastProcessedId,
                batchSize);

            if (result.ReadRows == 0) return;

            await SaveStateAsync(new BuildCatalogueCandidatesState
            {
                LastProcessedId = result.LastProcessedId,
                ProcessedRows = State.ProcessedRows + result.ReadRows,
                AssignedRows = State.AssignedRows + result.AssignedRows,
                SkippedRows = State.SkippedRows + result.SkippedRows
            });

            if (!result.HasMore) break;
        }
    }

    private Task<BatchResult> ProcessBatchAsync(
        int lastProcessedId,
        int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(lastProcessedId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        return TransactionService.ExecuteAsync(
            BatchTransactionSettings,
            async (context, cancellationToken) =>
            {
                var supplierProducts = await supplierProductRepository.ListAsync(
                    Criteria<SupplierProduct>
                        .New()
                        .Where(x => x.CatalogueCandidateId == null)
                        .Where(x => x.Id > lastProcessedId)
                        .OrderByAsc(x => x.Id)
                        .Track()
                        .Size(batchSize)
                        .Build(),
                    cancellationToken);

                if (supplierProducts.Count == 0)
                    return new BatchResult(
                        lastProcessedId,
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
                    await context.UnitOfWork.AddRangeAsync(
                        missingCandidates,
                        cancellationToken);
                    await context.UnitOfWork.SaveChangesAsync(cancellationToken);

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
                    persistedCandidates[key].AddSupplierProduct(
                        resolved.Product);
                }

                await context.UnitOfWork.SaveChangesAsync(cancellationToken);

                return new BatchResult(
                    supplierProducts[^1].Id,
                    supplierProducts.Count,
                    resolvedProducts.Count,
                    supplierProducts.Count - resolvedProducts.Count,
                    supplierProducts.Count == batchSize);
            },
            CancellationToken);
    }

    private sealed record ResolvedSupplierProduct(
        SupplierProduct Product,
        int? ProducerId);

    private sealed record BatchResult(
        int LastProcessedId,
        int ReadRows,
        int AssignedRows,
        int SkippedRows,
        bool HasMore);
}
