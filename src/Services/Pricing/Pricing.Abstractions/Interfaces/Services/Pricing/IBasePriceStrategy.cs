using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IBasePriceStrategy
{
    ArticlePricingType Type { get; }
    decimal GetPrice(IEnumerable<ArticlePrice> prices);
}