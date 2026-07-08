using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Settings;
using Pricing.Application.Extensions;
using Pricing.Application.Handlers.Pricing;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Entities;
using Pricing.Entities.Settings;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class PriceCandidateBuilder(
    ICurrencyConverter currencyConverter,
    ISettingsService settingsService) : IPriceCandidateBuilder
{
    public async Task<IReadOnlyCollection<PriceCandidate>> Build(
        IReadOnlyCollection<PriceOffer> offers,
        string targetStorageName,
        CancellationToken cancellationToken = default)
    {
        var baseCurrency = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken)).Data.BaseCurrencyId;
        var result = new List<PriceCandidate>();

        foreach (var offer in offers)
        {
            var sourceType = offer.Source.GetSourceType();
            result.Add(new PriceCandidate(
                PriceOfferId: offer.Id,
                ProductId: offer.ProductId,
                TargetStorageName: targetStorageName,
                SourceType: sourceType,
                Cost: offer.Price,
                CostCurrencyId: offer.CurrencyId,
                BaseCurrencyId: baseCurrency,
                CostInBaseCurrency: await currencyConverter.ConvertToBaseAsync(offer.Price, offer.CurrencyId, cancellationToken),
                AvailableQuantity: offer.AvailableQuantity,
                Fulfillment: sourceType == PriceOfferSourceType.OurWarehouse 
                    ? FulfillmentRouteInfo.SameStorage(targetStorageName)
                    : FulfillmentRouteInfo.FromSupplier(offer)));
        }
        
        return result;
    }
}