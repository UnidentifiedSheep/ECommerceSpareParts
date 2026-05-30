using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class MetricConverterDispatcher(IServiceProvider provider) : IMetricConverterDispatcher
{
    public Metric Convert(MetricPayloadDto payload, Type metricType)
    {
        var converter = GetConverter(metricType)
                        ?? throw new InvalidOperationException($"Metric converter {metricType} is not registered");

        return converter.Convert(payload);
    }

    private IMetricConverter? GetConverter(Type metricType)
    {
        var validatorType = typeof(IMetricConverter<>).MakeGenericType(metricType);
        var validator = provider.GetService(validatorType) as IMetricConverter;
        return validator;
    }
}