using Main.Enums;

namespace Main.Abstractions.Interfaces.Pricing;

public interface IBasePriceStrategyFactory
{
    IBasePriceStrategy GetStrategy(ArticlePricingType type);
    bool TryGetStrategy(ArticlePricingType type, out IBasePriceStrategy? strategy);
}