using Analytics.Entities.Metrics;

namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricCalculator<in T> where T : Metric
{
    Type MetricType { get; }
    Task CalculateMetric(T metric, CancellationToken cancellationToken = default);
}