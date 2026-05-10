using Main.Application.Models.Logistics;
using Main.Enums;

namespace Main.Application.Interfaces.Logistics;

public interface ILogisticsCostService
{
    LogisticsCalcResult Calculate(LogisticPricingType type, LogisticsContext context, IEnumerable<LogisticsItem> items);
}