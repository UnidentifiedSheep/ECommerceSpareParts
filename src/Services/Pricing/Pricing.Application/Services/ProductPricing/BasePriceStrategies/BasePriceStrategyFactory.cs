using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class BasePriceStrategyFactory : IBasePriceStrategyFactory
{
    private readonly Dictionary<ProductPricingType, IBasePriceStrategy> _strategies;

    public BasePriceStrategyFactory(IEnumerable<IBasePriceStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Type);
    }

    public IBasePriceStrategy GetStrategy(ProductPricingType type)
    {
        if (!_strategies.TryGetValue(type, out var strategy))
            throw new ArgumentException($"Strategy for type {type} not found");
        return strategy;
    }

    public bool TryGetStrategy(ProductPricingType type, out IBasePriceStrategy? strategy)
    {
        return _strategies.TryGetValue(type, out strategy);
    }
}