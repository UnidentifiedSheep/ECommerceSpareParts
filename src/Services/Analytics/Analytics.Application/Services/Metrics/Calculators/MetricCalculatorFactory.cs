using Analytics.Application.Interfaces.Services.Metrics;

namespace Analytics.Application.Services.Metrics.Calculators;

public class MetricCalculatorFactory(IServiceProvider provider) : IMetricCalculatorFactory
{
    public IMetricCalculator GetCalculator(Type metricType)
    {
        return TryGetCalculator(metricType)
               ?? throw new NotSupportedException($"Metric type {metricType} is not supported");
    }

    public IMetricCalculator? TryGetCalculator(Type metricType)
    {
        var serviceType = typeof(IMetricCalculator<>).MakeGenericType(metricType);
        return provider.GetService(serviceType) as IMetricCalculator;
    }
}