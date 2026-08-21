using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Services;
using Attributes;
using MediatR;

namespace Application.Common.Handlers.Jobs;

[Diagnostics]
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
    IJobCreationDispatcher jobCreationDispatcher,
    IJobService jobService
    ) : ICommandHandler<TryEnqueueUniqJobCommand>
{
    public async Task<Unit> Handle(TryEnqueueUniqJobCommand request, CancellationToken cancellationToken)
    {
        var jobs = request.Items
            .Select(x => jobCreationDispatcher.Create(
                x.SystemName,
                x.InputState,
                x.MaxAttempts,
                x.NaturalKey))
            .ToList();

        await jobService.TryEnqueueJobsAsync(
            jobs,
            cancellationToken);

        return Unit.Value;
    }
}
