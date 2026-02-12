using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class MedianBasePriceStrategy : IBasePriceStrategy
{
    public ArticlePricingType Type => ArticlePricingType.Median;
    
    public decimal GetPrice(IEnumerable<ArticlePrice> prices)
    {
        var sorted = prices
            .Select(x => x.Price + x.DeliveryPrice)
            .OrderBy(x => x)
            .ToList();

        if (sorted.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        int count = sorted.Count;
        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        
        return sorted[count / 2];
    }
}