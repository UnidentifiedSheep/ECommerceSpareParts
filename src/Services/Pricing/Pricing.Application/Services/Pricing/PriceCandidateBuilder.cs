using Pricing.Application.Extensions;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Entities;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class PriceCandidateBuilder() : IPriceCandidateBuilder
{
    public async Task<IReadOnlyCollection<PriceCandidate>> Build(
        IReadOnlyCollection<PriceOffer> offers,
        string targetStorageName,
        CancellationToken cancellationToken = default)
    {
        var result = new List<PriceCandidate>();

        foreach (var offer in offers)
        {
            var sourceType = offer.Source.GetSourceType();
            result.Add(new PriceCandidate(
                PriceOfferId: offer.Id,
                ProductId: offer.ProductId,
                TargetStorageName: targetStorageName,
                SourceType: sourceType,
                Cost: offer.PurchasePrice,
                CurrencyId: offer.CurrencyId,
                AvailableQuantity: offer.AvailableQuantity,
                Fulfillment: sourceType == PriceOfferSourceType.OurWarehouse 
                    ? FulfillmentRouteInfo.SameStorage(targetStorageName)
                    : FulfillmentRouteInfo.FromSupplier(offer)));
        }
        
        return result;
    }
}