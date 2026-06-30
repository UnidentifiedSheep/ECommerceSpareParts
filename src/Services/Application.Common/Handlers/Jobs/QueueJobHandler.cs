using Abstractions.Interfaces.Persistence;
using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Application.Common.Projections;
using Attributes;
using Domain.CommonEntities;

namespace Application.Common.Handlers.Jobs;

[Transactional]
public sealed record QueueJobCommand : ICommand<QueueJobResult>
{
    public readonly IReadOnlyList<QueueJobItem> Jobs;
    public QueueJobCommand(string systemName,
        string inputState,
        int maxAttempts)
    {
        Jobs = new List<QueueJobItem> { new(systemName, inputState, maxAttempts) };
    }
    
    public QueueJobCommand(IEnumerable<QueueJobItem> jobs)
    {
        Jobs = jobs.ToList();
    }
    
}

public sealed record QueueJobItem(string SystemName, string InputState, int MaxAttempts);

public sealed record QueueJobResult(IReadOnlyList<JobDto> Jobs);

public sealed class QueueJobHandler(
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<QueueJobCommand, QueueJobResult>
{
    public async Task<QueueJobResult> Handle(
        QueueJobCommand request, 
        CancellationToken cancellationToken)
    {
        var toAdd = new List<Job>();
        foreach (var item in request.Jobs)
        {
            var lrt = registry.GetBySystemName(item.SystemName);
            lrt.ValidateState(item.InputState);

            var job = Job.Create(lrt.SystemName, item.MaxAttempts);
            job.SetState(item.InputState);
            toAdd.Add(job);
        }
        
        await unitOfWork.AddRangeAsync(toAdd, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new QueueJobResult(toAdd.Select(job => JobProjections.JobProjection.AsFunc()(job)).ToList());
    }
}