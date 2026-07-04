using Analytics.Application.Dtos.Metric;
using Analytics.Application.NamedObjects.Metrics;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Attributes;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListAvailableMetrics;

[Diagnostics(maxExecutionTimeMs: 40)]
public sealed record ListAvailableMetricsQuery : IQuery<ListAvailableMetricsResult>;

public sealed record ListAvailableMetricsResult(IReadOnlyList<MetricInfoDto> Metrics);

public class ListAvailableMetricsHandler(
    IScopedStringLocalizer localizer,
    INamedObjectRegistry<MetricDefinitionNamedObjectBase> registry,
    IScopedLocalizedJsonSerializer jsonSerializer
) : IQueryHandler<ListAvailableMetricsQuery, ListAvailableMetricsResult>
{
    public Task<ListAvailableMetricsResult> Handle(
        ListAvailableMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var result = registry.All
            .Select(x => new MetricInfoDto
            {
                SystemName = x.SystemName,
                Name = localizer[x.NameLocalizationKey],
                Description = localizer[x.DescriptionLocalizationKey],
                InputSchema = jsonSerializer.SerializeMetadata(x.InputType)
            })
            .ToList();

        return Task.FromResult(new ListAvailableMetricsResult(result));
    }
}