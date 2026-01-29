using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaPricing : LogisticsPricingStrategyBase
{
    public override LogisticPricingType Type => LogisticPricingType.PerArea;
    public override LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items)
    {
        return Iterate(context, items, input => CalculatePrice(input.AreaM3, context));
    }

    private decimal CalculatePrice(decimal area, LogisticsContext context)
    {
        return context.PriceM3 * area;
    }
}