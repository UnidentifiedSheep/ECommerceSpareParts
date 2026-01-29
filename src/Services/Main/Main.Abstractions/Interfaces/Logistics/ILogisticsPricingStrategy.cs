using Main.Abstractions.Models.Logistics;
using Main.Enums;

namespace Main.Abstractions.Interfaces.Logistics;

public interface ILogisticsPricingStrategy
{
    LogisticPricingType Type { get; }
    LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items);
}