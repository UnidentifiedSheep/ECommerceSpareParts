using Main.Abstractions.Models.Pricing;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Pricing;

public interface IBasePriceStrategy
{
    ArticlePricingType Type { get; }
    decimal GetPrice(IEnumerable<ArticlePrice> prices);
}