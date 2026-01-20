using Main.Abstractions.Models;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Logistics;

public interface ILogisticsCostService
{
    decimal Calculate(LogisticPricingType type, LogisticsContext context);
}