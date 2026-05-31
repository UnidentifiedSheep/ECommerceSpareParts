using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Extensions;

public static class MetricExtensions
{
    public static List<(Type Type, string SystemName)> GetAvailableMetrics()
    {
        return typeof(IMetricCalculator<>).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        !t.ContainsGenericParameters)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IMetricCalculator<>))
                .Select(i => i.GetGenericArguments()[0]))
            .Where(t => t.IsSubclassOf(typeof(Metric)) &&
                        !t.IsAbstract &&
                        !t.ContainsGenericParameters)
            .Distinct()
            .Select(t => (t, t.Name))
            .ToList();
    }
}
