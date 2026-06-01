using System.Data;
using Analytics.Application.Handlers.CalculationJob.CreateCalculationJob;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities.Metrics;
using Analytics.Entities.Settings;
using Analytics.Enums;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using MediatR;

namespace Analytics.Application.Handlers.Metrics.ScheduleDirtyMetricsRecalculation;

[Diagnostics(maxExecutionTimeMs: 500)]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record ScheduleDirtyMetricsRecalculationCommand(int Limit) : ICommand;

public class ScheduleDirtyMetricsRecalculationHandler(
    IMetricRepository metricRepository,
    ISettingsService settingsService,
    ISender sender
    ) : ICommandHandler<ScheduleDirtyMetricsRecalculationCommand>
{
    public async Task<Unit> Handle(
        ScheduleDirtyMetricsRecalculationCommand request, 
        CancellationToken cancellationToken)
    {
        var systemId = (await settingsService
                .GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
        
        var criteria = Criteria<Metric>.New()
            .Where(x => 
                (x.Tags & (RecalculationTags.RecalculationNeeded | RecalculationTags.Disabled))
                == RecalculationTags.RecalculationNeeded)
            .Where(x => 
                x.CalculationJobs
                    .All(z => z.Status != CalculationStatus.AwaitingWorker))
            .ForUpdate(true, true)
            .Track(false)
            .Size(request.Limit)
            .Build();
        
        var metrics = await metricRepository.ListAsync(criteria, cancellationToken);
        foreach (var metric in metrics)
            await sender.Send(
                new CreateCalculationJobCommand(metric.Id, systemId),
                cancellationToken);
        
        return Unit.Value;
    }
}