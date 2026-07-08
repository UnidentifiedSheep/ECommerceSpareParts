using Application.Common.Interfaces.Settings;
using Pricing.Application.Interfaces.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Application.Models.Pricing.MarketInfo;
using Pricing.Application.Models.Pricing.PriceCandidates;
using Pricing.Enums;

namespace Pricing.Application.Services.Pricing;

public sealed class MarketInfoFactory(
    IOfferScorer offerScorer) : IMarketInfoFactory
{
    public async Task<MarketInfo> CreateFromSupplierPrices(
        IReadOnlyCollection<CalculatedPriceCandidate> calculatedSupplierCandidates)
    {
        var candidates = calculatedSupplierCandidates
            .Where(x => x.SourceType == PriceOfferSourceType.Supplier)
            .Where(x => x.AvailableQuantity > 0)
            .Where(x => x.CostInBaseCurrency > 0)
            .Where(x => x.PriceInBaseCurrency > 0)
            .ToArray();

        if (candidates.Length == 0) return MarketInfo.Empty;

        decimal minCost = decimal.MaxValue, maxCost = decimal.MinValue, totalCost = 0;
        decimal minPrice = decimal.MaxValue, maxPrice = decimal.MinValue, totalPrice = 0;
        int availableQuantity = 0;

        var items = new List<MarketInfoItem>();
        foreach (var candidate in candidates)
        {
            minCost = Math.Min(minCost, candidate.CostInBaseCurrency);
            maxCost = Math.Max(maxCost, candidate.CostInBaseCurrency);
            totalCost += candidate.CostInBaseCurrency;

            minPrice = Math.Min(minPrice, candidate.PriceInBaseCurrency);
            maxPrice = Math.Max(maxPrice, candidate.PriceInBaseCurrency);
            totalPrice += candidate.PriceInBaseCurrency;
            
            availableQuantity += candidate.AvailableQuantity;
            items.Add(new MarketInfoItem
            {
                Cost = candidate.CostInBaseCurrency,
                DeliveryTime = candidate.DeliveryTime,
                Score = await offerScorer.GetCostScoreAsync(new OfferCostScoreContext
                {
                    Cost = candidate.CostInBaseCurrency,
                    DeliveryDays = (int)Math.Ceiling(candidate.DeliveryTime.TotalDays),
                    GuaranteedDeliveryDays = (int)Math.Ceiling(candidate.GuaranteedDeliveryTime.TotalDays)
                })
            });
        }
        
        return new MarketInfo(items)
        {
            MinCost = minCost,
            AverageCost = totalCost / candidates.Length,
            MaxCost = maxCost,

            MinPrice = minPrice,
            AveragePrice = totalPrice / candidates.Length,
            MaxPrice = maxPrice,

            OfferCount = candidates.Length,
            AvailableQuantity = availableQuantity
        };
    }
}