using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Attributes;
using Cronos;
using Domain.CommonEntities.Job;
using FluentValidation;

namespace Application.Common.Services.Job;

public class JobScheduleService(
    IApplicationTransactionService transactionService,
    INamedObjectRegistry<ILrtNamedObject> registry,
    IJobService jobService,
    IValidator<NewJobScheduleDto> newScheduleValidator,
    IValidator<PatchJobScheduleDto> patchScheduleValidator,
    TimeProvider timeProvider) : IJobScheduleService
{
    public async Task<Guid> CreateScheduleAsync(
        NewJobScheduleDto newSchedule,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(newSchedule);
        await newScheduleValidator.ValidateAndThrowAsync(newSchedule, token);

        return await transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var lrt = registry.GetBySystemName(
                    newSchedule.JobSystemName);
                var validatedState = lrt.ValidateState(
                    newSchedule.InputState);

                var schedule = JobSchedule.Create(
                    newSchedule.Name,
                    newSchedule.Description,
                    lrt.SystemName,
                    validatedState,
                    newSchedule.MaxAttempts,
                    newSchedule.Cron);

                if (newSchedule.Enabled)
                    schedule.Enable();

                var nextRunAt = CronExpression.Parse(schedule.Cron)
                    .GetNextOccurrence(
                        GetUtcNow(),
                        JobSchedule.TimeZone);

                schedule.SetNextRunAt(nextRunAt);

                await ctx.UnitOfWork.AddAsync(schedule, ct);
                await ctx.UnitOfWork.SaveChangesAsync(ct);

                return schedule.Id;
            },
            cancellationToken: token);
    }

    public async Task<Guid> UpdateScheduleAsync(
        Guid scheduleId,
        PatchJobScheduleDto patch,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(patch);
        await patchScheduleValidator.ValidateAndThrowAsync(patch, token);

        return await transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var repository = ctx.Repositories
                    .Get<JobSchedule, Guid>();
                var schedule = await GetForUpdateAsync(repository, scheduleId, ct)
                               ?? throw new JobScheduleNotFoundException(
                                   scheduleId);

                var nextRunAtMustBeRecalculated = false;

                patch.Name.Apply(schedule.SetName);
                patch.Description.Apply(schedule.SetDescription);
                patch.MaxAttempts.Apply(schedule.SetMaxAttempts);

                if (patch.InputState.IsSet)
                {
                    var lrt = registry.GetBySystemName(
                        schedule.JobSystemName);
                    var validatedState = lrt.ValidateState(
                        patch.InputState.Value!);

                    schedule.SetInputState(validatedState);
                }

                if (patch.Cron.IsSet)
                {
                    schedule.SetCron(patch.Cron.Value!);
                    nextRunAtMustBeRecalculated = true;
                }

                if (patch.Enabled.IsSet)
                {
                    if (patch.Enabled.Value)
                    {
                        schedule.Enable();
                        nextRunAtMustBeRecalculated = true;
                    }
                    else
                    {
                        schedule.Disable();
                    }
                }

                if (nextRunAtMustBeRecalculated)
                    schedule.SetNextRunAt(GetNextRunAt(schedule));

                await ctx.UnitOfWork.SaveChangesAsync(ct);
                return schedule.Id;
            },
            cancellationToken: token);
    }

    public Task RemoveScheduleAsync(
        Guid scheduleId,
        CancellationToken token = default)
    {
        return transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var repository = ctx.Repositories
                    .Get<JobSchedule, Guid>();
                var schedule = await GetForUpdateAsync(repository, scheduleId, ct)
                               ?? throw new JobScheduleNotFoundException(
                                   scheduleId);

                ctx.UnitOfWork.Remove(schedule);
                await ctx.UnitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken: token);
    }

    public Task<int> QueueDueSchedulesAsync(
        int batchSize,
        CancellationToken token = default)
    {
        if (batchSize <= 0) return Task.FromResult(0);

        return transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var utcNow = GetUtcNow();
                var repository = ctx.Repositories.Get<JobSchedule, Guid>();
                var criteria = Criteria<JobSchedule>.New()
                    .Where(x => x.Enabled)
                    .Where(x => x.NextRunAt != null && x.NextRunAt <= utcNow)
                    .OrderByAsc(x => x.NextRunAt)
                    .ForUpdate(true, true)
                    .Track()
                    .Size(batchSize)
                    .Build();

                var schedules = await repository.ListAsync(criteria, ct);
                if (schedules.Count == 0) return 0;

                var jobIds = await jobService.TryEnqueueJobsAsync(
                    schedules.Select(x => new JobItem(
                        x.JobSystemName,
                        x.InputState,
                        x.MaxAttempts)),
                    ct);

                if (jobIds.Count != schedules.Count)
                    throw new InvalidOperationException(
                        "All non-unique scheduled jobs must be enqueued.");

                for (var i = 0; i < schedules.Count; i++)
                {
                    var schedule = schedules[i];
                    var scheduledAt = schedule.NextRunAt!.Value;
                    var nextRunAt = CronExpression.Parse(schedule.Cron)
                        .GetNextOccurrence(utcNow, JobSchedule.TimeZone);

                    schedule.MarkQueued(utcNow, nextRunAt);
                    schedule.AddScheduleRun(jobIds[i], scheduledAt, utcNow);
                }

                await ctx.UnitOfWork.SaveChangesAsync(ct);
                return schedules.Count;
            },
            cancellationToken: token);
    }

    private DateTime? GetNextRunAt(JobSchedule schedule)
    {
        return CronExpression.Parse(schedule.Cron)
            .GetNextOccurrence(
                GetUtcNow(),
                JobSchedule.TimeZone);
    }

    private DateTime GetUtcNow()
    {
        return timeProvider.GetUtcNow().UtcDateTime;
    }

    private static Task<JobSchedule?> GetForUpdateAsync(
        IRepository<JobSchedule, Guid> repository,
        Guid scheduleId,
        CancellationToken token)
    {
        var criteria = Criteria<JobSchedule>.New()
            .Where(x => x.Id == scheduleId)
            .Track()
            .ForUpdate()
            .Build();

        return repository.FirstOrDefaultAsync(criteria, token);
    }
}
