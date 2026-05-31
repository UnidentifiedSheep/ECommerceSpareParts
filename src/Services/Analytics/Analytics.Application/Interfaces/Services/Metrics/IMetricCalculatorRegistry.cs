using Analytics.Entities.Metrics;

namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricCalculatorRegistry
{
    Type GetMetricType(string name);
    string GetSystemName<TMetric>() where TMetric : Metric;
    string GetSystemName(Type type);
    bool TryGetMetricType(string name, out Type? type);
}