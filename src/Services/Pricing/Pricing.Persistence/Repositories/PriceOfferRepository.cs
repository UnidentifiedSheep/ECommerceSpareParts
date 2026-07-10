using Abstractions.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using Pricing.Application.Interfaces.Persistence;
using Pricing.Entities;
using Pricing.Entities.Offers;
using Pricing.Enums;
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
        var allOffers = offers.ToList();

        allOffers.ForEach(x => x.Touch(userContext.UserIdOrNull));
        
        await Context.BulkInsertOrUpdateAsync(
            allOffers,
            new BulkConfig
            {
                UpdateByProperties =
                [
                    nameof(PriceOffer.ProductId),
                    nameof(PriceOffer.Source),
                    nameof(PriceOffer.SourceKey),
                    nameof(PriceOffer.OfferForStorage)
                ],

                PropertiesToIncludeOnUpdate =
                [
                    nameof(PriceOffer.CurrencyId),
                    nameof(PriceOffer.PurchasePrice),
                    nameof(PriceOffer.AvailableQuantity),
                    nameof(PriceOffer.MinimumPurchaseQuantity),
                    nameof(PriceOffer.QuantityCoefficient),
                    nameof(PriceOffer.DaysToRefund),
                    nameof(PriceOffer.DeliveryDate),
                    nameof(PriceOffer.GuaranteedDeliveryDate),
                    nameof(PriceOffer.DeliveryProbability),
                    nameof(PriceOffer.OrderTill),
                    nameof(PriceOffer.ExpiresAt),
                    nameof(PriceOffer.SourceOccurredAt),
                    nameof(PriceOffer.UpdatedAt),
                    nameof(PriceOffer.WhoUpdated)
                ],
                
                OnConflictUpdateWhereSql = (existingTable, insertedTable) =>
                    $"{insertedTable}.source_occurred_at IS NULL " +
                    $"OR {existingTable}.source_occurred_at IS NULL " +
                    $"OR {insertedTable}.source_occurred_at > {existingTable}.source_occurred_at"
            },
            cancellationToken: cancellationToken);
    }
    public Task DeleteOffersAsync(
        int productId,
        string storageName,
        IEnumerable<PriceOfferSource> sources,
        CancellationToken cancellationToken = default)
        => Context.PriceOffers
            .Where(x => x.ProductId == productId)
            .Where(x => x.OfferForStorage == storageName)
            .Where(x => sources.Contains(x.Source))
            .ExecuteDeleteAsync(cancellationToken);
}
