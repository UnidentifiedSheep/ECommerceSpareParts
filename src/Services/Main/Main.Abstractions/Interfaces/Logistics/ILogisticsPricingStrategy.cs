using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Logistics;

public interface ILogisticsPricingStrategy
{
    LogisticPricingType Type { get; }
    decimal Calculate(LogisticsContext context);
}