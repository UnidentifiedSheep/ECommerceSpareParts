using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;

namespace Application.Common.Handlers.JobSchedules.UpdateSchedule;

public record UpdateScheduleCommand(
    Guid ScheduleId,
    PatchJobScheduleDto Patch
) : ICommand<UpdateScheduleResult>;

public record UpdateScheduleResult(Guid ScheduleId);

public class UpdateScheduleHandler(
    IJobScheduleService jobScheduleService
) : ICommandHandler<UpdateScheduleCommand, UpdateScheduleResult>
{
    public async Task<UpdateScheduleResult> Handle(
        UpdateScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var scheduleId = await jobScheduleService.UpdateScheduleAsync(
            request.ScheduleId,
            request.Patch,
            cancellationToken);

        return new UpdateScheduleResult(scheduleId);
    }
}
