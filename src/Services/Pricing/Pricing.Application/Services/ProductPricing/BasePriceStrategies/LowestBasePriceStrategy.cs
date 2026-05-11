using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class LowestBasePriceStrategy : IBasePriceStrategy
{
    public ProductPricingType Type => ProductPricingType.Lowest;

    public decimal GetPrice(IEnumerable<ProductPrice> prices)
    {
        var list = prices.ToList();
        if (list.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        return list.Min(x => x.Price + x.DeliveryPrice);
    }
}