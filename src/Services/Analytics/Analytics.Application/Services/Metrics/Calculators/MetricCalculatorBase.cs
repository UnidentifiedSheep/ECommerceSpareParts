using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Calculators;

public abstract class MetricCalculatorBase<T> : IMetricCalculator<T> where T : Metric
{
    public Type MetricType => typeof(T);
    public Task CalculateMetric(object metric, CancellationToken cancellationToken = default)
    {
        if (metric is not T typedMetric)
            throw new InvalidOperationException($"Cannot calculate metric of type {metric.GetType().Name}.\n" +
                                                $"Expected type {typeof(T).Name} but got {metric.GetType().Name}");
        
        return CalculateMetric(typedMetric, cancellationToken);
    }

    public abstract Task CalculateMetric(T metric, CancellationToken cancellationToken = default);

    protected async Task<(DateTime start, DateTime end)> WithTimer(Func<Task> func)
    {
        var start = DateTime.UtcNow;
        await func();
        var end = DateTime.UtcNow;
        return (start, end);
    }
}