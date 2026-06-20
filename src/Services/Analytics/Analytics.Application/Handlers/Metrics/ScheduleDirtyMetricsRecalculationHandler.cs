using System.Data;
using System.Text.Json;
using Analytics.Application.Interfaces.Repositories;
using Analytics.Application.Lrts.MetricCalculation;
using Analytics.Entities.Metrics;
using Analytics.Enums;
using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEnums;
using MediatR;

namespace Analytics.Application.Handlers.Metrics;

[AutoSave]
[Diagnostics(maxExecutionTimeMs: 500)]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record ScheduleDirtyMetricsRecalculationCommand(int Limit) : ICommand;

public class ScheduleDirtyMetricsRecalculationHandler(
    IMetricRepository metricRepository,
    ISender sender
    ) : ICommandHandler<ScheduleDirtyMetricsRecalculationCommand>
{
    public async Task<Unit> Handle(
        ScheduleDirtyMetricsRecalculationCommand request, 
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<Metric>.New()
            .Where(x => 
                (x.Tags & (RecalculationTags.RecalculationNeeded | RecalculationTags.Disabled))
                == RecalculationTags.RecalculationNeeded)
            .Where(x => 
                x.Jobs
                    .All(z => z.Job.Status != JobStatus.Pending 
                              && z.Job.Status != JobStatus.Locked
                              && z.Job.Status != JobStatus.Processing))
            .ForUpdate(true, true)
            .Track()
            .Size(request.Limit)
            .Build();
        
        var metrics = await metricRepository.ListAsync(criteria, cancellationToken);
        var jobItems = metrics
            .Select(x => new QueueJobItem(
                SystemName: MetricCalculationLrt.LrtSystemName, 
                InputState: GetInputState(x.Id), 
                MaxAttempts: 3))
            .ToList();
        
        var jobResult = await sender.Send(new QueueJobCommand(jobItems), cancellationToken);
        
        for (int i = 0; i < metrics.Count; i++)
            metrics[i].AddJob(jobResult.Jobs[i].Id);
        
        return Unit.Value;
    }

    private static string GetInputState(Guid metricId)
        => JsonSerializer.Serialize(new MetricCalculationInputState { MetricId = metricId });
}
