using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public abstract class MetricConverterBase<TMetric> : IMetricConverter<TMetric> where TMetric : Metric
{
    public abstract TMetric Convert(MetricPayloadDto payload);

    Metric IMetricConverter.Convert(MetricPayloadDto payload)
    {
        return Convert(payload);
    }

    protected static void FillBase(Metric metric, MetricPayloadDto payload)
    {
        metric.ConfigurePeriod(payload.CurrencyId, payload.RangeStart, payload.RangeEnd);
    }
}
