using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricConverter<out TMetric> : IMetricConverter where TMetric : Metric
{
    new TMetric Convert(MetricPayloadDto payload);
}

public interface IMetricConverter
{
    Metric Convert(MetricPayloadDto payload);
}