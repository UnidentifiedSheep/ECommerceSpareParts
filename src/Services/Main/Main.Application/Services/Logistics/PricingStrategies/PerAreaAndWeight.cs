using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaAndWeight : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.PerAreaAndWeight;
    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        return Iterate(context, items, input => CalculatePrice(input.AreaM3, input.WeightKg, context));
    }

    private decimal CalculatePrice(decimal area, decimal weight, LogisticsContext context)
    {
        return (context.PriceM3 * area) + (weight * context.PriceKg);
    }
}