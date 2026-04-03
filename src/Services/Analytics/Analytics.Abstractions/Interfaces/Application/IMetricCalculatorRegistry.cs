namespace Analytics.Abstractions.Interfaces.Application;

public interface IMetricCalculatorRegistry
{
    Type GetMetricType(string name);
    bool TryGetMetricType(string name, out Type? type);
}