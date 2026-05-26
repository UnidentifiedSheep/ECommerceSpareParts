using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services;

public interface IMetricConverterDispatcher
{
    Metric Convert(MetricPayloadDto payload, Type metricType);
}