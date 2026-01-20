using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerAreaAndWeight : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.PerAreaAndWeight;
    public decimal Calculate(LogisticsContext context) => (context.PriceM3 * context.AreaM3) +
                                                          (context.WightKg * context.PriceKg);
}