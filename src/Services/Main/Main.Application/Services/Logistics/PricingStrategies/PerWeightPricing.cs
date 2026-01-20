using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerWeightPricing : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.PerWeight;
    public decimal Calculate(LogisticsContext context) => context.PriceKg * context.WightKg;
}