using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Services;
using JobItemModel = Application.Common.Models.Jobs.JobItem;

namespace Application.Common.Handlers.Jobs;

public sealed record QueueJobCommand : ICommand<QueueJobResult>
{
    public readonly IReadOnlyList<QueueJobItem> Jobs;

    public QueueJobCommand(
        string systemName,
        string inputState,
        int maxAttempts)
    {
        Jobs = new List<QueueJobItem>
        {
            new(
                systemName,
                inputState,
                maxAttempts)
        };
    }

    public QueueJobCommand(IEnumerable<QueueJobItem> jobs) { Jobs = jobs.ToList(); }
}

public sealed record QueueJobItem(
    string SystemName,
    string InputState,
    int MaxAttempts
);

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
            request.Jobs.Select(x => new JobItemModel(
                x.SystemName,
                x.InputState,
                x.MaxAttempts)),
            cancellationToken);

        return new QueueJobResult(addedIds);
    }
}
