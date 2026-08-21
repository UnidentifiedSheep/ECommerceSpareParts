using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using MediatR;

namespace Application.Common.Handlers.Jobs;

public record CancelJobCommand(Guid JobId) : ICommand;

public class CancelJobHandler(
    IJobService jobService) : ICommandHandler<CancelJobCommand>
{
    public async Task<Unit> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        await jobService.CancelJobAsync(
            request.JobId,
            cancellationToken);

        return Unit.Value;
    }
}
