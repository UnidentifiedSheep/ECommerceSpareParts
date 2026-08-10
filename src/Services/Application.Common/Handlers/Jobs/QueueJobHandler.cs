using Abstractions.Interfaces.Persistence;
using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;

namespace Application.Common.Handlers.Jobs;

[Transactional]
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
    IJobCreationDispatcher jobCreationDispatcher,
    IUnitOfWork unitOfWork
) : ICommandHandler<QueueJobCommand, QueueJobResult>
{
    public async Task<QueueJobResult> Handle(
        QueueJobCommand request,
        CancellationToken cancellationToken)
    {
        var toAdd = new List<Job>();
        foreach (var item in request.Jobs)
            toAdd.Add(jobCreationDispatcher.Create(
                item.SystemName,
                item.InputState,
                item.MaxAttempts));

        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new QueueJobResult(toAdd.Select(x => x.Id).ToList());
    }
}
