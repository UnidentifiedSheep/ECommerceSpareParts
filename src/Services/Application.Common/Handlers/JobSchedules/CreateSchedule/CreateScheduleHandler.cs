using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;

namespace Application.Common.Handlers.JobSchedules.CreateSchedule;

public record CreateScheduleCommand(NewJobScheduleDto NewSchedule) : ICommand<CreateScheduleResult>;

public record CreateScheduleResult(Guid ScheduleId);

public class CreateScheduleHandler(
    IJobScheduleService jobScheduleService
) : ICommandHandler<CreateScheduleCommand, CreateScheduleResult>
{
    public async Task<CreateScheduleResult> Handle(
        CreateScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var scheduleId = await jobScheduleService.CreateScheduleAsync(
            request.NewSchedule,
            cancellationToken);

        return new CreateScheduleResult(scheduleId);
    }
}
