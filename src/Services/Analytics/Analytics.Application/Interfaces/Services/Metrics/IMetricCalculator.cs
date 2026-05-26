using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services;

public interface IMetricCalculator<in T> : IMetricCalculator where T : Metric
{
    Task CalculateMetric(T metric, CancellationToken cancellationToken = default);
}

public interface IMetricCalculator
{
    Type MetricType { get; }
    Task CalculateMetric(object metric, CancellationToken cancellationToken = default);
}