using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricConverterDispatcher
{
    Metric FromPayload(MetricPayloadDto payload, Type metricType);
    MetricPayloadDto ToPayload(Metric metric);
    IMetricConverter? GetConverter(Type metricType);
}