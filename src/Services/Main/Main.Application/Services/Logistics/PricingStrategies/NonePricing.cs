using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Application.Services.Logistics.PricingStrategies;

public class NonePricing : ILogisticsPricingStrategy
{
    public LogisticPricingType Type => LogisticPricingType.None;
    public decimal Calculate(LogisticsContext context) => 0;
}