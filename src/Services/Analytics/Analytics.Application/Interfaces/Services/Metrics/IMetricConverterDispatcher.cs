using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricConverterDispatcher
{
    Metric Convert(MetricPayloadDto payload, Type metricType);
}