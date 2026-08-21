using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using Attributes;
using MediatR;

namespace Application.Common.Handlers.JobSchedules;

[Diagnostics]
public record RemoveJobScheduleCommand(Guid JobScheduleId) : ICommand;

public class RemoveJobScheduleHandler(
    IJobScheduleService jobScheduleService
) : ICommandHandler<RemoveJobScheduleCommand>
{
    public async Task<Unit> Handle(RemoveJobScheduleCommand request, CancellationToken cancellationToken)
    {
        await jobScheduleService.RemoveScheduleAsync(
            request.JobScheduleId,
            cancellationToken);

        return Unit.Value;
    }
}
