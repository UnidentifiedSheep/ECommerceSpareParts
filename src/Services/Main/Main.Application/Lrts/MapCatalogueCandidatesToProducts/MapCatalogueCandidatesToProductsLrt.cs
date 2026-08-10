using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Attributes;
using Domain.CommonEntities.Job;
using MassTransit;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product.Enrichment;
using Microsoft.Extensions.Logging;

namespace Main.Application.Lrts.MapCatalogueCandidatesToProducts;

public class MapCatalogueCandidatesToProductsLrt(
    IRepository<Job, Guid> jobRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publisher,
    IApplicationTransactionService transactionService,
    IRepository<CatalogueCandidate, int> catalogueCandidateRepository,
    IProductRepository productRepository,
    ILogger<MapCatalogueCandidatesToProductsLrt> logger)
    : LrtBase<NoneInputState, MapCatalogueCandidatesToProductsState>(
    jobRepository, 
    unitOfWork,
    publisher, 
    transactionService,
    logger)
{
    public const string LrtSystemName = nameof(MapCatalogueCandidatesToProductsLrt);
    public override string SystemName => LrtSystemName;
    public override string NameLocalizationKey => "lrt.catalogue.candidates.map.to.products.name";
    public override string DescriptionLocalizationKey => "lrt.catalogue.candidates.map.to.products.description";

    protected override async Task DoWork()
    {
        const int batchSize = 1000;

        while (true)
        {
            var result = await ProcessBatchAsync(
                State.LastProcessedId,
                batchSize);

            if (result.ReadRows == 0) return;

            await SaveStateAsync(new MapCatalogueCandidatesToProductsState
            {
                LastProcessedId = result.LastProcessedId,
                ProcessedRows = State.ProcessedRows + result.ReadRows,
                MappedRows = State.MappedRows + result.MappedRows,
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
            TransactionalAttribute.ReadCommited(30,3),
            async (context, cancellationToken) =>
            {
                var candidates = await catalogueCandidateRepository.ListAsync(
                    Criteria<CatalogueCandidate>
                        .New()
                        .Where(x => x.ProductId == null)
                        .Where(x => x.Id > lastProcessedId)
                        .OrderByAsc(x => x.Id)
                        .Track()
                        .Size(batchSize)
                        .Build(),
                    cancellationToken);

                if (candidates.Count == 0)
                    return new BatchResult(
                        lastProcessedId,
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

                await context.UnitOfWork.SaveChangesAsync(cancellationToken);

                return new BatchResult(
                    candidates[^1].Id,
                    candidates.Count,
                    mappedRows,
                    candidates.Count - mappedRows,
                    candidates.Count == batchSize);
            },
            CancellationToken);
    }

    private sealed record BatchResult(
        int LastProcessedId,
        int ReadRows,
        int MappedRows,
        int SkippedRows,
        bool HasMore);
}
