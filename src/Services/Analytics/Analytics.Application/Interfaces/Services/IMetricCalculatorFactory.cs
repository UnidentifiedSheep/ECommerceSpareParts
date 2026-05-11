using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services;

public interface IMetricCalculatorFactory
{
    IMetricCalculator<TMetric>? TryGetCalculator<TMetric>() where TMetric : Metric;
    IMetricCalculator? TryGetCalculator(Type metricType);

    IMetricCalculator GetCalculator(Type metricType);
    IMetricCalculator<TMetric> GetCalculator<TMetric>() where TMetric : Metric;
}