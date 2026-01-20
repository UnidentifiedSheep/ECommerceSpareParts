using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaOrWeightPricing : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.PerAreaOrWeight;
    public decimal Calculate(LogisticsContext context) 
        => Math.Max(context.PriceM3 * context.AreaM3, context.PriceKg * context.WightKg);
}