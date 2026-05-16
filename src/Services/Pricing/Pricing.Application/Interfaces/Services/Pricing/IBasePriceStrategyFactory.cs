using Pricing.Enums;

namespace Pricing.Application.Interfaces.Services.Pricing;

public interface IBasePriceStrategyFactory
{
    IBasePriceStrategy GetStrategy(ProductPricingType type);
    bool TryGetStrategy(ProductPricingType type, out IBasePriceStrategy? strategy);
}