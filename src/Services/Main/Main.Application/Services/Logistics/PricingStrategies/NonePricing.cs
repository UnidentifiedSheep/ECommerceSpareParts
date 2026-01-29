using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class NonePricing : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.None;

    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        return Iterate(context, items, (_) => 0);
    }
}