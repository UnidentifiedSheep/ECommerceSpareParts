using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class LowestBasePriceStrategy : IBasePriceStrategy
{
    public ArticlePricingType Type => ArticlePricingType.Lowest;
    
    public decimal GetPrice(IEnumerable<ArticlePrice> prices)
    {
        var list = prices.ToList();
        if (list.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        return list.Min(x => x.Price + x.DeliveryPrice);
    }
}