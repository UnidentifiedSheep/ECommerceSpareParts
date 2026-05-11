namespace Analytics.Application.Interfaces.Services;

public interface IMetricCalculatorRegistry
{
    Type GetMetricType(string name);
    bool TryGetMetricType(string name, out Type? type);
}