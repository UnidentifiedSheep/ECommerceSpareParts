using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Projections;

public static class MetricProjection
{
    public static Expression<Func<Metric, MetricDto>> ToDto(
        IReadOnlyDictionary<string, MetricInfoDto> infos,
        IScopedLocalizedJsonSerializer serializer) =>
        x => new MetricDto
        {
            Id = x.Id,
            SystemName = x.Discriminator,
            Description = infos[x.Discriminator].Description,
            Name = infos[x.Discriminator].Name,
            Data = serializer.Serialize(x.GetData()),
            Tags = x.Tags,
            RangeEnd = x.RangeEnd,
            RangeStart = x.RangeStart,
            CurrencyId = x.CurrencyId,
            ProductId = x is ProductPurchasesMetric ? ((ProductPurchasesMetric)x).ProductId
                : x is ProductSalesMetric ? ((ProductSalesMetric)x).ProductId : null
        };
}