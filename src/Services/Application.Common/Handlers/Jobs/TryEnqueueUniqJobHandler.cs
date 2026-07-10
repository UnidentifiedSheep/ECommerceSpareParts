using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Attributes;
using Domain.CommonEntities;
using MediatR;

namespace Application.Common.Handlers.Jobs;

[Diagnostics]
[Transactional, AutoSave]
public record TryEnqueueUniqJobCommand : ICommand
{
    public IReadOnlyList<TryEnqueueUniqJobItem> Items { get; }
    
    public TryEnqueueUniqJobCommand(IEnumerable<TryEnqueueUniqJobItem> items)
    {
        Items = items.ToList();
    }
    
    public TryEnqueueUniqJobCommand(TryEnqueueUniqJobItem item)
    {
        Items = [item];
    }
}

public record TryEnqueueUniqJobItem(
    string NaturalKey,
    string SystemName,
    string InputState,
    int MaxAttempts);

public class TryEnqueueUniqJobHandler(
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IJobRepository jobRepository
    ) : ICommandHandler<TryEnqueueUniqJobCommand>
{
    public async Task<Unit> Handle(TryEnqueueUniqJobCommand request, CancellationToken cancellationToken)
    {
        var toAdd = new List<UniqJob>();
        foreach (var item in request.Items)
        {
            var lrt = registry.GetBySystemName(item.SystemName);

            var job = UniqJob.Create(
                item.NaturalKey,
                lrt.SystemName,
                lrt.ValidateState(item.InputState), 
                item.MaxAttempts);
            toAdd.Add(job);
        }
        
        await jobRepository.TryInsertPendingUniqueAsync(toAdd, cancellationToken);
        return Unit.Value;
    }
}