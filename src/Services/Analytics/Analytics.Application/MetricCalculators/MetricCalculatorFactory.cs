using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application.MetricCalculators;

public class MetricCalculatorFactory(IServiceScope scope) : IMetricCalculatorFactory
{
    public IMetricCalculator<TMetric>? TryGetCalculator<TMetric>() where TMetric : Metric
    {
        return scope.ServiceProvider.GetService<IMetricCalculator<TMetric>>();
    }

    public IMetricCalculator? TryGetCalculator(Type metricType)
    {
        var serviceType = typeof(IMetricCalculator<>).MakeGenericType(metricType);
        return scope.ServiceProvider.GetService(serviceType) as IMetricCalculator;
    }
    
    public IMetricCalculator GetCalculator(Type metricType)
        => TryGetCalculator(metricType) 
           ?? throw new NotSupportedException($"Metric type {metricType} is not supported");
    
    public IMetricCalculator<TMetric> GetCalculator<TMetric>() where TMetric : Metric
        => TryGetCalculator<TMetric>() 
           ?? throw new NotSupportedException($"Metric type {typeof(TMetric).Name} is not supported");
}