using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Handlers.Projections;

public static class MetricProjection
{
    public static Expression<Func<Metric, MetricDto>> ToDto(IReadOnlyDictionary<string, MetricInfoDto> infos) =>
        x => new MetricDto
        {
            Id = x.Id,
            SystemName = x.Discriminator,
            Description = infos[x.Discriminator].Description,
            Name = infos[x.Discriminator].Name,
            Data = x.Json
        };
}