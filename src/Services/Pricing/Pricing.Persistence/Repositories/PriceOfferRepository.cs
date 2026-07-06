using Abstractions.Interfaces;
using EFCore.BulkExtensions;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Persistence.Contexts;
using IQueryableExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Pricing.Persistence.Repositories;

public class PriceOfferRepository(
    DContext context, 
    IUserContext userContext,
    IQueryableExtensions extensions
    ) : LinqRepositoryBase<DContext, PriceOffer, Guid>(context, extensions), IPriceOfferRepository
{
    public async Task UpsertOffersAsync(
        IEnumerable<PriceOffer> offers,
        CancellationToken cancellationToken = default)
    {
        var distinctOffers = offers
            .GroupBy(x => new { x.Source, x.SourceKey })
            .Select(x => x.Last())
            .ToList();
        
        if (distinctOffers.Count == 0) return;

        distinctOffers.ForEach(x => x.Touch(userContext.UserIdOrNull));
        
        await Context.BulkInsertOrUpdateAsync(
            distinctOffers,
            new BulkConfig
            {
                UpdateByProperties =
                [
                    nameof(PriceOffer.Source),
                    nameof(PriceOffer.SourceKey)
                ],

                PropertiesToIncludeOnUpdate =
                [
                    nameof(PriceOffer.ProductId),
                    nameof(PriceOffer.CurrencyId),
                    nameof(PriceOffer.Price),
                    nameof(PriceOffer.AvailableQuantity),
                    nameof(PriceOffer.MinimumPurchaseQuantity),
                    nameof(PriceOffer.QuantityCoefficient),
                    nameof(PriceOffer.DaysToRefund),
                    nameof(PriceOffer.DeliveryDate),
                    nameof(PriceOffer.GuaranteedDeliveryDate),
                    nameof(PriceOffer.DeliveryProbability),
                    nameof(PriceOffer.OrderTill),
                    nameof(PriceOffer.ExpiresAt),
                    nameof(PriceOffer.UpdatedAt),
                    nameof(PriceOffer.WhoUpdated)
                ]
            },
            cancellationToken: cancellationToken);
    }
}