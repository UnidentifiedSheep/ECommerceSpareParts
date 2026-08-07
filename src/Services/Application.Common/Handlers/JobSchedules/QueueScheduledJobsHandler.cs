using Application.Common.Handlers.Jobs;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Cronos;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using MediatR;

namespace Application.Common.Handlers.JobSchedules;

[Diagnostics]
[Transactional]
[AutoSave]
public record QueueScheduledJobsCommand(int BatchSize) : ICommand<QueueScheduledJobsResult>;

public record QueueScheduledJobsResult(int Queued);

public class QueueScheduledJobsHandler(
    IRepository<JobSchedule, Guid> repository,
    ISender sender
) : ICommandHandler<QueueScheduledJobsCommand, QueueScheduledJobsResult>
{
    public async Task<QueueScheduledJobsResult> Handle(
        QueueScheduledJobsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.BatchSize <= 0) return new QueueScheduledJobsResult(0);

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
        if (schedules.Count == 0) return new QueueScheduledJobsResult(0);

        var jobs = (await sender.Send(
            new QueueJobCommand(
                schedules
                    .Select(x => new QueueJobItem(
                        x.JobSystemName,
                        x.InputState,
                        x.MaxAttempts))
                    .ToList()),
            cancellationToken)).JobIds;

        for (var i = 0; i < schedules.Count; i++)
        {
            var schedule = schedules[i];
            var job = jobs[i];

            var scheduledAt = schedule.NextRunAt!.Value;

            var nextRunAt = CronExpression.Parse(schedule.Cron)
                .GetNextOccurrence(
                    uncorrectedTime,
                    JobSchedule.TimeZone);

            schedule.MarkQueued(uncorrectedTime, nextRunAt);
            schedule.AddScheduleRun(
                job,
                scheduledAt,
                uncorrectedTime);
        }

        return new QueueScheduledJobsResult(schedules.Count);
    }
}
