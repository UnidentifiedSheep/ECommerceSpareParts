using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricConverter<TMetric> : IMetricConverter where TMetric : Metric
{
    new TMetric FromPayload(MetricPayloadDto payload);
    MetricPayloadDto ToPayload(TMetric metric);
}

public interface IMetricConverter
{
    Metric FromPayload(MetricPayloadDto payload);
    MetricPayloadDto ToPayload(Metric metric);
}