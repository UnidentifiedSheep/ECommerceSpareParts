using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Attributes;
using Domain.CommonEntities;

namespace Application.Common.Handlers.Jobs;

[Transactional]
public sealed record QueueJobCommand(
    string SystemName,
    string InputState,
    int MaxAttempts) : ICommand<QueueJobResult>;

public sealed record QueueJobResult(Guid JobId);

public sealed class QueueJobHandler(
    INamedObjectRegistry<LrtNamedObjectBase> registry,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<QueueJobCommand, QueueJobResult>
{
    public async Task<QueueJobResult> Handle(
        QueueJobCommand request, 
        CancellationToken cancellationToken)
    {
        var lrt = registry.GetBySystemName(request.SystemName);
        var validatedState = InputStateValidator.GetAndValidate(lrt.InputType, request.InputState);

        var job = Job.Create(lrt.SystemName, request.MaxAttempts);
        job.SetState(validatedState);

        await unitOfWork.AddAsync(job, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new QueueJobResult(job.Id);
    }
}