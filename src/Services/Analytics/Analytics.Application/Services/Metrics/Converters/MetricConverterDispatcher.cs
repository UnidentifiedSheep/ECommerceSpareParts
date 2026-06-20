using Analytics.Application.Dtos.Metric;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class MetricConverterDispatcher(IServiceProvider provider) : IMetricConverterDispatcher
{
    public Metric FromPayload(MetricPayloadDto payload, Type metricType)
    {
        var converter = GetConverter(metricType)
                        ?? throw new InvalidOperationException($"Metric converter {metricType} is not registered");

        return converter.FromPayload(payload);
    }

    public MetricPayloadDto ToPayload(Metric metric)
    {
        var converter = GetConverter(metric.GetType())
            ?? throw new InvalidOperationException($"Metric converter {metric.GetType()} is not registered");
        
        return converter.ToPayload(metric);
    }

    public IMetricConverter? GetConverter(Type metricType)
    {
        var validatorType = typeof(IMetricConverter<>).MakeGenericType(metricType);
        var validator = provider.GetService(validatorType) as IMetricConverter;
        return validator;
    }
}