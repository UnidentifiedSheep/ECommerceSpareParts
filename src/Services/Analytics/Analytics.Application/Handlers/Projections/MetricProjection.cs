using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Projections;

public static class MetricProjection
{
    public static Expression<Func<Metric, MetricDto>> ToDto(IReadOnlyDictionary<string, MetricInfoDto> infos) =>
        x => new MetricDto
        {
            SystemName = x.Discriminator,
            Description = infos[x.Discriminator].Description,
            Name = infos[x.Discriminator].Name,
        };
}