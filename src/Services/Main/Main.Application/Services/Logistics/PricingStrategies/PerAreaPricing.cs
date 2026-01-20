using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaPricing : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.PerArea;
    public decimal Calculate(LogisticsContext context) => context.PriceM3 * context.AreaM3;
}