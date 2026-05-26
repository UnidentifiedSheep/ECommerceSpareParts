using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services;

public interface IMetricConverter<out TMetric> : IMetricConverter where TMetric : Metric
{
    new TMetric Convert(MetricPayloadDto payload);
}

public interface IMetricConverter
{
    Metric Convert(MetricPayloadDto payload);
}