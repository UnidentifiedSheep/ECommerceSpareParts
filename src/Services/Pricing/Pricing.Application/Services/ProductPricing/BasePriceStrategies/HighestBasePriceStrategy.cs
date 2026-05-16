using Pricing.Application.Interfaces.Services.Pricing;
using Pricing.Application.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ProductPricing.BasePriceStrategies;

public class HighestBasePriceStrategy : IBasePriceStrategy
{
    public ProductPricingType Type => ProductPricingType.Highest;

    public decimal GetPrice(IEnumerable<ProductPrice> prices)
    {
        var list = prices.ToList();
        if (list.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        return list.Max(x => x.Price + x.DeliveryPrice);
    }
}