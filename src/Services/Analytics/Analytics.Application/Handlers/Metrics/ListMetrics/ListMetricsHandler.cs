using System.Reflection;
using Analytics.Abstractions.Dtos.Metric;
using Analytics.Attributes;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListMetrics;

public sealed record ListMetricsQuery() : IQuery<ListMetricsResult>;

public sealed record ListMetricsResult(IReadOnlyList<MetricInfoDto> Metrics);


public class ListMetricsHandler(IScopedStringLocalizer localizer) : IQueryHandler<ListMetricsQuery, ListMetricsResult>
{
    public Task<ListMetricsResult> Handle(ListMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = GetAllMetricWithInfos();

        var result = metrics.Select(attribute => 
            new MetricInfoDto
            {
                SystemName = attribute.SystemName, 
                Description = localizer[attribute.DescriptionLocalizationKey], 
                Name = localizer[attribute.NameLocalizationKey]
            }).ToList();

        return Task.FromResult(new ListMetricsResult(result));
    }

    private MetricInfoAttribute[] GetAllMetricWithInfos()
    {
        return typeof(Metric).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Metric).IsAssignableFrom(t))
            .Select(t => new
            {
                Info = t.GetCustomAttribute<MetricInfoAttribute>()
            })
            .Where(x => x.Info != null)
            .Select(x => x.Info!)
            .ToArray();
    }
}