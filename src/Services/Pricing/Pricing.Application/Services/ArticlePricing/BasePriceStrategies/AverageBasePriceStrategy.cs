using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class AverageBasePriceStrategy : IBasePriceStrategy
{
    public ArticlePricingType Type => ArticlePricingType.Average;

    public decimal GetPrice(IEnumerable<ArticlePrice> prices)
    {
        var list = prices.ToList();
        if (list.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        decimal priceSum = 0;
        decimal pricesCount = 0;
        
        foreach (var price in list)
        {
            priceSum += price.DeliveryPrice;
            priceSum += price.Price;
            pricesCount++;
        }
        
        return priceSum / pricesCount;
    }
}