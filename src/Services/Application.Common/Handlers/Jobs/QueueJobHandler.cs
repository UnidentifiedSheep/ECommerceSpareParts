using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using Application.Common.Models;

namespace Application.Common.Handlers.Jobs;

public sealed record QueueJobCommand : ICommand<QueueJobResult>
{
    public readonly IReadOnlyList<JobItem> Jobs;

    public QueueJobCommand(
        string systemName,
        string inputState,
        int maxAttempts)
    {
        Jobs = new List<JobItem>
        {
            new(
                systemName,
                inputState,
                maxAttempts)
        };
    }

    public QueueJobCommand(IEnumerable<JobItem> jobs) { Jobs = jobs.ToList(); }
}

public sealed record QueueJobResult(IReadOnlyList<Guid> JobIds);

public sealed class QueueJobHandler(
    IJobService jobService
) : ICommandHandler<QueueJobCommand, QueueJobResult>
{
    public async Task<QueueJobResult> Handle(
        QueueJobCommand request,
        CancellationToken cancellationToken)
    {
        var addedIds = await jobService.TryEnqueueJobsAsync(
            request.Jobs,
            cancellationToken);

        return new QueueJobResult(addedIds);
    }
}
