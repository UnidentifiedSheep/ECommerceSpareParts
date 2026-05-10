using Main.Application.Models.Logistics;
using Main.Enums;

namespace Main.Application.Interfaces.Logistics;

public interface ILogisticsPricingStrategy
{
    LogisticPricingType Type { get; }
    LogisticsCalcResult Calculate(LogisticsContext context, IEnumerable<LogisticsItem> items);
}