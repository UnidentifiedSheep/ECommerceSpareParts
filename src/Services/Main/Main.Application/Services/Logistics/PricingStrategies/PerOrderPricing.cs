using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerOrderPricing : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.PerOrder;
    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        var result = Iterate(context, items, _ => 0, LogisticsDataRequirements.None);
        result.TotalCost = context.PricePerOrder;
        return result;
    }
}