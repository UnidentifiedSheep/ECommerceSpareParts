using EFCore.BulkExtensions;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Persistence.Contexts;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Pricing.Persistence.Repositories;

public class PriceRecalculationRequestRepository(
    DContext context,
    IQueryableExtensions extensions
) : LinqRepositoryBase<DContext, PriceRecalculationRequest, PriceRecalculationRequestKey>(context, extensions),
    IPriceRecalculationRequestRepository
{
    public async Task UpsertAsync(
        IEnumerable<PriceRecalculationRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        var requestsList = requests
            .DistinctBy(x => new { x.ProductId, x.StorageName })
            .ToList();

        if (requestsList.Count == 0) return;

        await Context.BulkInsertOrUpdateAsync(
            requestsList,
            new BulkConfig
            {
                UpdateByProperties =
                [
                    nameof(PriceRecalculationRequest.ProductId),
                    nameof(PriceRecalculationRequest.StorageName)
                ],

                PropertiesToIncludeOnUpdate =
                [
                    nameof(PriceRecalculationRequest.UpdatedAt)
                ]
            },
            cancellationToken: cancellationToken);
    }
}
