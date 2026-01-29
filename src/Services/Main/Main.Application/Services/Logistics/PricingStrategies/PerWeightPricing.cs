using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerWeightPricing : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.PerWeight;
    
    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        return Iterate(context, items, input => CalculatePrice(input.WeightKg, context));
    }

    private decimal CalculatePrice(decimal weight, LogisticsContext context)
    {
        return context.PriceKg * weight;
    }
}