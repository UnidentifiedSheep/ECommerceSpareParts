using Pricing.Application.Interfaces.Services.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ProductPricing.BasePriceStrategies;

public class MedianBasePriceStrategy : IBasePriceStrategy
{
    public ProductPricingType Type => ProductPricingType.Median;

    public decimal GetPrice(IEnumerable<ProductPrice> prices)
    {
        var sorted = prices
            .Select(x => x.Price + x.DeliveryPrice)
            .OrderBy(x => x)
            .ToList();

        if (sorted.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        var count = sorted.Count;
        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;

        return sorted[count / 2];
    }
}