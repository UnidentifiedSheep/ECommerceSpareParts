using Application.Common.Interfaces.Currency;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class MarketInfoFactory(
    IOfferScorer offerScorer,
    ICurrencyConverter currencyConverter
    ) : IMarketInfoFactory
{
    public async Task<MarketInfo> CreateFromSupplierPrices(
        IReadOnlyCollection<CalculatedPriceCandidate> calculatedSupplierCandidates)
    {
        var candidates = calculatedSupplierCandidates
            .Where(x => x.SourceType == PriceOfferSourceType.Supplier)
            .Where(x => x.AvailableQuantity > 0)
            .Where(x => x.Cost > 0)
            .Where(x => x.Price > 0)
            .ToArray();

        if (candidates.Length == 0) return MarketInfo.Empty;

        int availableQuantity = 0;

        var items = new List<MarketInfoItem>();
        foreach (var candidate in candidates)
        {
            var costInBase = await currencyConverter.ConvertToBaseAsync(candidate.Cost, candidate.CurrencyId);
            availableQuantity += candidate.AvailableQuantity;
            items.Add(new MarketInfoItem
            {
                CostInBaseCurrency = costInBase,
                DeliveryTime = candidate.DeliveryTime,
                Score = await offerScorer.GetCostScoreAsync(new OfferCostScoreContext
                {
                    CostInBase = costInBase,
                    DeliveryDays = (int)Math.Ceiling(candidate.DeliveryTime.TotalDays),
                    GuaranteedDeliveryDays = (int)Math.Ceiling(candidate.GuaranteedDeliveryTime.TotalDays)
                })
            });
        }
        
        return new MarketInfo(items)
        {
            OfferCount = candidates.Length,
            AvailableQuantity = availableQuantity
        };
    }
}