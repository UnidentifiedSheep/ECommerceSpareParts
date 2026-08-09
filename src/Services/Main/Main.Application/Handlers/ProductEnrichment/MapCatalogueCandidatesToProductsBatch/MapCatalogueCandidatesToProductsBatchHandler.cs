using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product.Enrichment;

namespace Main.Application.Handlers.ProductEnrichment.MapCatalogueCandidatesToProductsBatch;

[AutoSave]
[Transactional]
public sealed record MapCatalogueCandidatesToProductsBatchCommand(
    int LastProcessedId,
    int BatchSize)
    : ICommand<MapCatalogueCandidatesToProductsBatchResult>;

public sealed record MapCatalogueCandidatesToProductsBatchResult(
    int LastProcessedId,
    int ReadRows,
    int MappedRows,
    int SkippedRows,
    bool HasMore);

public sealed class MapCatalogueCandidatesToProductsBatchHandler(
    IRepository<CatalogueCandidate, int> catalogueCandidateRepository,
    IProductRepository productRepository)
    : ICommandHandler<
        MapCatalogueCandidatesToProductsBatchCommand,
        MapCatalogueCandidatesToProductsBatchResult>
{
    public async Task<MapCatalogueCandidatesToProductsBatchResult> Handle(
        MapCatalogueCandidatesToProductsBatchCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(request.LastProcessedId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.BatchSize);

        var candidates = await catalogueCandidateRepository.ListAsync(
            Criteria<CatalogueCandidate>
                .New()
                .Where(x => x.ProductId == null)
                .Where(x => x.Id > request.LastProcessedId)
                .OrderByAsc(x => x.Id)
                .Track()
                .Size(request.BatchSize)
                .Build(),
            cancellationToken);

        if (candidates.Count == 0)
            return new MapCatalogueCandidatesToProductsBatchResult(
                request.LastProcessedId,
                0,
                0,
                0,
                false);

        var productIds = await productRepository.GetProductIdsByKeys(
            candidates.Select(x => (
                NormalizedSku: x.Sku.NormalizedValue,
                x.ProducerId)),
            cancellationToken);

        var mappedRows = 0;
        foreach (var candidate in candidates)
        {
            if (!productIds.TryGetValue(
                    (candidate.Sku.NormalizedValue, candidate.ProducerId),
                    out var productId))
                continue;

            candidate.MapToProduct(productId);
            mappedRows++;
        }

        return new MapCatalogueCandidatesToProductsBatchResult(
            candidates[^1].Id,
            candidates.Count,
            mappedRows,
            candidates.Count - mappedRows,
            candidates.Count == request.BatchSize);
    }
}
