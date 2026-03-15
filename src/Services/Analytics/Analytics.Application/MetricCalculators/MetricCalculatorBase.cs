using System.Diagnostics;
using Analytics.Abstractions.Interfaces.Application;
using Analytics.Entities.Metrics;

namespace Analytics.Application.MetricCalculators;

public abstract class MetricCalculatorBase<T> : IMetricCalculator<T> where T : Metric
{
    public Type MetricType => typeof(T);
    public abstract Task CalculateMetric(T metric, CancellationToken cancellationToken = default);

    protected async Task<(DateTime start, DateTime end)> WithTimer(Func<Task> func)
    {
        DateTime start = DateTime.UtcNow;
        await func();
        DateTime end = DateTime.UtcNow;
        return (start, end);
    }
}