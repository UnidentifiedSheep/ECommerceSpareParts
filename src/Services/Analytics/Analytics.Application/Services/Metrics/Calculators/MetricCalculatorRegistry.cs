using System.Reflection;
using Analytics.Application.Interfaces.Services;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Attributes;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Calculators;

public class MetricCalculatorRegistry : IMetricCalculatorRegistry
{
    private readonly Dictionary<string, Type> _nameToType = new();
    private readonly Dictionary<Type, string> _typeToName = new();

    public MetricCalculatorRegistry(Assembly? assembly = null)
    {
        RegisterFromAssembly(assembly ?? Assembly.GetExecutingAssembly());
    }

    public Type GetMetricType(string name)
    {
        if (_nameToType.TryGetValue(name, out var type))
            return type;

        throw new NotSupportedException($"Metric '{name}' is not supported");
    }

    public string GetSystemName<TMetric>() where TMetric : Metric
        => GetSystemName(typeof(TMetric));

    public string GetSystemName(Type type)
    {
        return _typeToName.TryGetValue(type, out var systemName) 
            ? systemName 
            : throw new NotSupportedException($"Metric '{type.Name}' is not supported");
    }

    public bool TryGetMetricType(string name, out Type? type)
    {
        return _nameToType.TryGetValue(name, out type);
    }

    private void RegisterFromAssembly(Assembly assembly)
    {
        var result = assembly.GetTypes()
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IMetricCalculator<>))
                .Select(i => new
                {
                    CalculatorType = t,
                    MetricType = i.GetGenericArguments()[0]
                }))
            .Where(x =>
                x.MetricType.IsSubclassOf(typeof(Metric)) &&
                x.MetricType.GetCustomAttribute<MetricInfoAttribute>() != null)
            .Select(x => new
            {
                Type = x.MetricType,
                x.MetricType.GetCustomAttribute<MetricInfoAttribute>()!.SystemName
            });

        foreach (var type in result)
        {
            _nameToType.Add(type.SystemName, type.Type);
            _typeToName.Add(type.Type, type.SystemName);
        }
    }
}