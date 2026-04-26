using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Logistics;

public interface ILogisticsCostService
{
    LogisticsCalcResult Calculate(LogisticPricingType type, LogisticsContext context, IEnumerable<LogisticsItem> items);
}