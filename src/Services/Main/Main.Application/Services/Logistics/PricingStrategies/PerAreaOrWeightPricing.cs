using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaOrWeightPricing : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.PerAreaOrWeight;
    
    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        return Iterate(context, items, input => CalculatePrice(input.AreaM3, input.WeightKg, context));
    }

    private decimal CalculatePrice(decimal area, decimal weight, LogisticsContext context)
    {
        return Math.Max(context.PriceM3 * area, context.PriceKg * weight);
    }
}