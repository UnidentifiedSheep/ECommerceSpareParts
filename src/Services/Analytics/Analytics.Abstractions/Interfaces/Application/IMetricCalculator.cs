using Analytics.Entities.Metrics;

namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricCalculator<in T> : IMetricCalculator where T : Metric
{
    Task CalculateMetric(T metric, CancellationToken cancellationToken = default);
}

public interface IMetricCalculator
{
    Type MetricType { get; }
    Task CalculateMetric(object metric, CancellationToken cancellationToken = default);
}