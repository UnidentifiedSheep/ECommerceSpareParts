using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class PerOrderPricing : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.PerOrder;
    public decimal Calculate(LogisticsContext context) => context.PricePerOrder;
}