using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Attributes;
using Cronos;
using Domain.CommonEntities.Job;

namespace Application.Common.Services.Job;

public class JobScheduleService(
    IApplicationTransactionService transactionService,
    INamedObjectRegistry<ILrtNamedObject> registry) : IJobScheduleService
{
    public Task<Guid> CreateScheduleAsync(
        NewJobScheduleDto newSchedule,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(newSchedule);

        return transactionService.ExecuteAsync(
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
                        DateTime.UtcNow,
                        JobSchedule.TimeZone);

                schedule.SetNextRunAt(nextRunAt);

                await ctx.UnitOfWork.AddAsync(schedule, ct);
                await ctx.UnitOfWork.SaveChangesAsync(ct);

                return schedule.Id;
            },
            cancellationToken: token);
    }

    public Task<Guid> UpdateScheduleAsync(
        Guid scheduleId,
        PatchJobScheduleDto patch,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(patch);

        return transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var repository = ctx.Repositories
                    .Get<JobSchedule, Guid>();
                var schedule = await repository.GetById(scheduleId, ct)
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
                var schedule = await repository.GetById(scheduleId, ct)
                               ?? throw new JobScheduleNotFoundException(
                                   scheduleId);

                ctx.UnitOfWork.Remove(schedule);
                await ctx.UnitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken: token);
    }

    private static DateTime? GetNextRunAt(JobSchedule schedule)
    {
        return CronExpression.Parse(schedule.Cron)
            .GetNextOccurrence(
                DateTime.UtcNow,
                JobSchedule.TimeZone);
    }
}
