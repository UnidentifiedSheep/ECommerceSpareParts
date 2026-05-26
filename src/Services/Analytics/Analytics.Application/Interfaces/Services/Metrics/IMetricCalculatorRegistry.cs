namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricCalculatorRegistry
{
    Type GetMetricType(string name);
    bool TryGetMetricType(string name, out Type? type);
}