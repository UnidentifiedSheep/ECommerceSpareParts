using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricConverterDispatcher
{
    Metric Convert(MetricPayloadDto payload, Type metricType);
}