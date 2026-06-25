using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Cronos;
using Domain.CommonEntities;
using MediatR;

namespace Application.Common.Handlers.JobSchedules;

[Diagnostics]
[Transactional, AutoSave]
public record QueueScheduledJobsCommand(int BatchSize) : ICommand;

public class QueueScheduledJobsHandler(
    IRepository<JobSchedule, Guid> repository,
    ISender sender) : ICommandHandler<QueueScheduledJobsCommand>
{
    public async Task<Unit> Handle(
        QueueScheduledJobsCommand request, 
        CancellationToken cancellationToken)
    {
        if (request.BatchSize <= 0)
            return Unit.Value;
        
        var uncorrectedTime = DateTime.UtcNow;
        
        var criteria = Criteria<JobSchedule>.New()
            .Where(x => x.Enabled)
            .Where(x => x.NextRunAt != null && x.NextRunAt <= uncorrectedTime)
            .OrderByAsc(x => x.NextRunAt)
            .ForUpdate(true, true)
            .Track()
            .Size(request.BatchSize)
            .Build();

        var schedules = await repository.ListAsync(criteria, cancellationToken);
        if (schedules.Count == 0)
            return Unit.Value;

        var jobs = (await sender.Send(new QueueJobCommand(
                schedules
                    .Select(x => new QueueJobItem(x.JobSystemName, x.InputState, x.MaxAttempts))
                    .ToList()),
            cancellationToken)).Jobs;

        for (var i = 0; i < schedules.Count; i++)
        {
            var schedule = schedules[i];
            var job = jobs[i];
            
            var nextRunAt = CronExpression.Parse(schedule.Cron)
                .GetNextOccurrence(
                    uncorrectedTime,
                    JobSchedule.TimeZone);

            schedule.MarkQueued(uncorrectedTime, nextRunAt);
            schedule.AddScheduleRun(job.Id, schedule.NextRunAt!.Value, uncorrectedTime);
        }

        return Unit.Value;
    }
}
