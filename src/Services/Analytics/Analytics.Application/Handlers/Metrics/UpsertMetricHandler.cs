using Abstractions.Interfaces.Persistence;
using Analytics.Application.Dtos.Metric;
using Analytics.Application.Handlers.Metrics.ListAvailableMetrics;
using Analytics.Application.Handlers.Projections;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.NamedObjects.Metrics;
using Analytics.Entities.Exceptions;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Localization.Abstractions.Interfaces;
using MediatR;

namespace Analytics.Application.Handlers.Metrics;

[Diagnostics(maxExecutionTimeMs: 150)]
[Transactional, AutoSave]
public record UpsertMetricCommand(
    string MetricSystemName,
    string InputPayload) : ICommand<UpsertMetricResult>;

public record UpsertMetricResult(MetricDto Metric);

public class UpsertMetricHandler(
    IMetricRepository metricRepository,
    ISender sender,
    IUnitOfWork unitOfWork,
    INamedObjectRegistry<MetricDefinitionNamedObjectBase> registry,
    IScopedLocalizedJsonSerializer serializer)
    : ICommandHandler<UpsertMetricCommand, UpsertMetricResult>
{
    public async Task<UpsertMetricResult> Handle(UpsertMetricCommand request, CancellationToken cancellationToken)
    {
        var metricDefinition = registry.TryGetBySystemName(request.MetricSystemName)
            ?? throw new MetricNotFoundException();
        var metric = metricDefinition.CreateMetricUntyped(request.InputPayload);

        var criteria = Criteria<Metric>.New()
            .Where(x => x.NaturalKey == Metric.GetNaturalKey(metric))
            .Track()
            .Build();
        
        var existingMetric = await metricRepository
            .FirstOrDefaultAsync(criteria, cancellationToken);
        if (existingMetric is not null)
            return new UpsertMetricResult(MetricProjection.ToDto(await GetMetricInfos(cancellationToken), serializer)
                .AsFunc()(existingMetric));

        metric.MarkDirty();
        await unitOfWork.AddAsync(metric, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new UpsertMetricResult(MetricProjection.ToDto(await GetMetricInfos(cancellationToken), serializer)
            .AsFunc()(metric));
    }
    
    private async Task<IReadOnlyDictionary<string, MetricInfoDto>> GetMetricInfos(CancellationToken cancellationToken)
    {
        return (await sender.Send(new ListAvailableMetricsQuery(), cancellationToken))
            .Metrics
            .ToDictionary(x => x.SystemName);
    }
}
