using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Abstractions.Interfaces.Application;
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
        var validatorType = typeof(IMetricConverter).MakeGenericType(metricType);
        var validator = provider.GetService(validatorType) as IMetricConverter;
        return validator;
    }
}