using Pricing.Enums;

namespace Pricing.Abstractions.Interfaces.Services.Pricing;

public interface IBasePriceStrategyFactory
{
    IBasePriceStrategy GetStrategy(ProductPricingType type);
    bool TryGetStrategy(ProductPricingType type, out IBasePriceStrategy? strategy);
}