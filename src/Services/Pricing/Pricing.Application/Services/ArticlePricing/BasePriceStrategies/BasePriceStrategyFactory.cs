using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Services.ArticlePricing.BasePriceStrategies;

public class BasePriceStrategyFactory : IBasePriceStrategyFactory
{
    private readonly Dictionary<ArticlePricingType, IBasePriceStrategy> _strategies;
    public BasePriceStrategyFactory(IEnumerable<IBasePriceStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Type);
    }
    
    public IBasePriceStrategy GetStrategy(ArticlePricingType type)
    {
        if (!_strategies.TryGetValue(type, out var strategy))
            throw new ArgumentException($"Strategy for type {type} not found");
        return strategy;
    }
    
    public bool TryGetStrategy(ArticlePricingType type, out IBasePriceStrategy? strategy) 
        => _strategies.TryGetValue(type, out strategy);
}