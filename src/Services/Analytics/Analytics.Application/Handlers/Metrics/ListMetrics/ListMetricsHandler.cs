using System.Reflection;
using Analytics.Application.Dtos.Metric;
using Analytics.Attributes;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.Cqrs;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListMetrics;

public sealed record ListMetricsQuery : IQuery<ListMetricsResult>;

public sealed record ListMetricsResult(IReadOnlyList<MetricInfoDto> Metrics);

public class ListMetricsHandler(IScopedStringLocalizer localizer) : IQueryHandler<ListMetricsQuery, ListMetricsResult>
{
    private static readonly MetricInfoAttribute[] MetricsInfo = typeof(Metric).Assembly.GetTypes()
        .Where(t => !t.IsAbstract && typeof(Metric).IsAssignableFrom(t))
        .Select(t => new
        {
            Info = t.GetCustomAttribute<MetricInfoAttribute>()
        })
        .Where(x => x.Info != null)
        .Select(x => x.Info!)
        .ToArray();
    public Task<ListMetricsResult> Handle(ListMetricsQuery request, CancellationToken cancellationToken)
    {
        var result = MetricsInfo.Select(attribute =>
            new MetricInfoDto
            {
                SystemName = attribute.SystemName,
                Description = localizer[attribute.DescriptionLocalizationKey],
                Name = localizer[attribute.NameLocalizationKey]
            }).ToList();

        return Task.FromResult(new ListMetricsResult(result));
    }
}