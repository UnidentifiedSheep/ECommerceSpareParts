using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.Cqrs;
using Attributes;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListAvailableMetrics;

[Diagnostics(maxExecutionTimeMs: 40)]
public sealed record ListAvailableMetricsQuery : IQuery<ListAvailableMetricsResult>;

public sealed record ListAvailableMetricsResult(IReadOnlyList<MetricInfoDto> Metrics);

public class ListAvailableMetricsHandler(IScopedStringLocalizer localizer) : IQueryHandler<ListAvailableMetricsQuery, ListAvailableMetricsResult>
{
    private static readonly (string systemName, string nameKey, string descriptionKey)[] MetricsInfo = 
        typeof(Metric).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Metric).IsAssignableFrom(t))
            .Select(t => (t.Name, $"{t.Name}.name", $"{t.Name}.description"))
            .ToArray();
    public Task<ListAvailableMetricsResult> Handle(ListAvailableMetricsQuery request, CancellationToken cancellationToken)
    {
        var result = MetricsInfo.Select(a =>
            new MetricInfoDto
            {
                SystemName = a.systemName,
                Description = localizer[a.descriptionKey],
                Name = localizer[a.nameKey]
            }).ToList();

        return Task.FromResult(new ListAvailableMetricsResult(result));
    }
}