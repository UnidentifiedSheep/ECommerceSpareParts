using Pricing.Enums;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IBasePriceStrategyFactory
{
    IBasePriceStrategy GetStrategy(ArticlePricingType type);
    bool TryGetStrategy(ArticlePricingType type, out IBasePriceStrategy? strategy);
}