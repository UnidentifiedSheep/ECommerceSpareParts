using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics;

public class LogisticsCostService : ILogisticsCostService
{
    private readonly Dictionary<LogisticPricingType, ILogisticsPricingStrategy> _strategies;
    
    public LogisticsCostService(IEnumerable<ILogisticsPricingStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Type);
    }

    /// <summary>
    /// If base price is less than minimal price, set base price to minimal price.
    /// </summary>
    /// <param name="basePrice"></param>
    /// <param name="minimalPrice"></param>
    /// <returns>true if minimal price applied</returns>
    private bool WithMinimalPrice(ref decimal basePrice, decimal? minimalPrice)
    {
        if (!minimalPrice.HasValue || basePrice >= minimalPrice.Value) return false;
        basePrice = minimalPrice.Value;
        return true;
    }

    public LogisticsCalcResult Calculate(LogisticPricingType type, LogisticsContext context, 
        IEnumerable<LogisticsItem> items)
    {
        var result = _strategies[type].Calculate(context, items);
        var totalCost = result.TotalCost;
        
        if (!WithMinimalPrice(ref totalCost, context.MinimumPrice)) return result;
        
        result.TotalCost = totalCost;
        result.MinimalPriceApplied = true;

        return result;
    }
}