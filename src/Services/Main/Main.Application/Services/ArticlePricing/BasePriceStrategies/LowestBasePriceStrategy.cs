using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Models.Pricing;
using Main.Enums;

namespace Main.Application.Services.ArticlePricing.BasePriceStrategies;

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