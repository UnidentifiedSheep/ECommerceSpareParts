using Abstractions.Interfaces.Persistence;
using Application.Common.Dtos;
using Application.Common.Exceptions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Application.Common.Projections;
using Attributes;
using Cronos;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Handlers.JobSchedules.UpdateSchedule;

[Transactional]
public record UpdateScheduleCommand(
    Guid ScheduleId,
    PatchJobScheduleDto Patch) : ICommand<UpdateScheduleResult>;

public record UpdateScheduleResult(JobScheduleDto Schedule);

public class UpdateScheduleHandler(
    IRepository<JobSchedule, Guid> repository,
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IScopedStringLocalizer localizer,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateScheduleCommand, UpdateScheduleResult>
{
    public async Task<UpdateScheduleResult> Handle(
        UpdateScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var schedule = await repository.GetById(request.ScheduleId, cancellationToken)
                       ?? throw new JobScheduleNotFoundException(request.ScheduleId);

        var patch = request.Patch;
        var nextRunAtMustBeRecalculated = false;

        patch.Name.Apply(schedule.SetName);
        patch.Description.Apply(schedule.SetDescription);
        patch.MaxAttempts.Apply(schedule.SetMaxAttempts);

        if (patch.InputState.IsSet)
        {
            var lrt = registry.GetBySystemName(schedule.JobSystemName);
            lrt.ValidateState(patch.InputState.Value!);
            
            schedule.SetInputState(patch.InputState.Value!);
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
        {
            var nextRunAt = CronExpression.Parse(schedule.Cron)
                .GetNextOccurrence(
                    DateTime.UtcNow,
                    JobSchedule.TimeZone);
            
            schedule.SetNextRunAt(nextRunAt);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateScheduleResult(JobProjections.JobScheduleProjection(localizer).AsFunc()(schedule));
    }
}
