using Analytics.Application.Dtos.Metric;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public abstract class MetricConverterBase<TMetric> : 
    IMetricConverter<TMetric> where TMetric : Metric
{
    public abstract TMetric FromPayload(MetricPayloadDto payload);
    public abstract MetricPayloadDto ToPayload(TMetric metric);

    public MetricPayloadDto ToPayload(Metric metric)
    {
        return ToPayload((TMetric)metric);
    }

    Metric IMetricConverter.FromPayload(MetricPayloadDto payload)
    {
        return FromPayload(payload);
    }

    protected static void FillBase(Metric metric, MetricPayloadDto payload)
    {
        metric.ConfigurePeriod(
            payload.CurrencyId, 
            payload.RangeStart, 
            payload.RangeEnd);
    }
}