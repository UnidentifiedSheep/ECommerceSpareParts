using System.Reflection;
using Analytics.Application.Interfaces.Services.Metrics;
using Analytics.Application.Services.Metrics.Validators;
using Analytics.Attributes;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Extensions;

public static class MetricExtensions
{
    public static List<(Type, string)> GetAvailableMetrics()
    {
        return typeof(MetricValidator).Assembly
            .GetTypes()
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
            .Select(x => (x.MetricType, x.MetricType.GetCustomAttribute<MetricInfoAttribute>()!.SystemName))
            .ToList();
    }
}