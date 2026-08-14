using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Application.NamedObjects.Metrics;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Projections;

[Lifetime(Lifetime.Scoped)]
public sealed class MetricDtoProjectionProvider
    : ProjectionProviderBase<Metric, MetricDto>
{
    public MetricDtoProjectionProvider(
        IScopedStringLocalizer localizer,
        IScopedLocalizedJsonSerializer serializer,
        INamedObjectRegistry<MetricDefinitionNamedObjectBase> registry,
        IProjectionProvider<MetricJob, MetricJobDto> metricJobProjection)
    {
        var names = registry.All.ToDictionary(
            x => x.SystemName,
            x => localizer[x.NameLocalizationKey]);
        var descriptions = registry.All.ToDictionary(
            x => x.SystemName,
            x => localizer[x.DescriptionLocalizationKey]);
        var metricJobToDto = metricJobProjection.Projection;
        Expression<Func<MetricJob?, MetricJobDto?>> metricJobToDtoOrDefault =
            x => x == null ? null : metricJobToDto.Invoke(x);

        Projection = x => new MetricDto
        {
            Id = x.Id,
            SystemName = x.Discriminator,
            Description = descriptions[x.Discriminator],
            Name = names[x.Discriminator],
            Data = serializer.Serialize(x.GetData()),
            Tags = x.Tags,
            RangeEnd = x.RangeEnd,
            RangeStart = x.RangeStart,
            CurrencyId = x.CurrencyId,
            ProductId = x is ProductPurchasesMetric ? ((ProductPurchasesMetric)x).ProductId :
                x is ProductSalesMetric ? ((ProductSalesMetric)x).ProductId : null,
            LastMetricJob = metricJobToDtoOrDefault.Invoke(
                x.Jobs
                    .OrderByDescending(z => z.Job.CreatedAt)
                    .FirstOrDefault())
        };
    }

    public override Expression<Func<Metric, MetricDto>> Projection { get; }
}
