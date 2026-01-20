using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics;

public class LogisticsCostService : ILogisticsCostService
{
    private readonly Dictionary<LogisticPricingType, ILogisticsPricingStrategy> _strategies;
    
    public LogisticsCostService(IEnumerable<ILogisticsPricingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Type);
    }

    public decimal Calculate(LogisticPricingType type, LogisticsContext context)
    {
        decimal calculatedPrice = _strategies[type].Calculate(context);
        WithMinimalPrice(ref calculatedPrice, context.MinimumPrice);
        
        
        return calculatedPrice;
    }


    private void WithMinimalPrice(ref decimal basePrice, decimal? minimalPrice) =>
        basePrice = Math.Max(basePrice, minimalPrice ?? 0);
}