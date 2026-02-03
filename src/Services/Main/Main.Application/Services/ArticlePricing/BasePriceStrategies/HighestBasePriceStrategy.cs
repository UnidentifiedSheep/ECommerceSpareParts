using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models.Pricing;
using Main.Enums;

namespace Main.Application.Services.ArticlePricing.BasePriceStrategies;

public class HighestBasePriceStrategy : IBasePriceStrategy
{
    public ArticlePricingType Type => ArticlePricingType.Highest;
    
    public decimal GetPrice(IEnumerable<ArticlePrice> prices)
    {
        var list = prices.ToList();
        if (list.Count == 0) throw new ArgumentException("Список с ценами не должен быть пуст.");

        return list.Max(x => x.Price + x.DeliveryPrice);
    }
}