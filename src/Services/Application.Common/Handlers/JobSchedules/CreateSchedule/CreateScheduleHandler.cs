using Abstractions.Interfaces.Persistence;
using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Application.Common.Projections;
using Attributes;
using Cronos;
using Domain.CommonEntities;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Handlers.JobSchedules.CreateSchedule;

[Transactional]
public record CreateScheduleCommand(NewJobScheduleDto NewSchedule) : IQuery<CreateScheduleResult>;

public record CreateScheduleResult(JobScheduleDto Schedule);

public class CreateScheduleHandler(
    IScopedStringLocalizer localizer,
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IUnitOfWork unitOfWork
) : IQueryHandler<CreateScheduleCommand, CreateScheduleResult>
{
    public async Task<CreateScheduleResult> Handle(
        CreateScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var lrt = registry.GetBySystemName(request.NewSchedule.JobSystemName);
        var validatedState = lrt.ValidateState(request.NewSchedule.InputState);

        var schedule = JobSchedule.Create(
            request.NewSchedule.Name,
            request.NewSchedule.Description,
            lrt.SystemName,
            validatedState,
            request.NewSchedule.MaxAttempts,
            request.NewSchedule.Cron);

        if (request.NewSchedule.Enabled) schedule.Enable();

        var nextRunAt = CronExpression.Parse(schedule.Cron)
            .GetNextOccurrence(
                DateTime.UtcNow,
                JobSchedule.TimeZone);

        schedule.SetNextRunAt(nextRunAt);

        await unitOfWork.AddAsync(schedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateScheduleResult(JobProjections.JobScheduleProjection(localizer).AsFunc()(schedule));
    }
}