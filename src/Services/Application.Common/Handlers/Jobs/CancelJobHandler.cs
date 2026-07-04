using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Job;
using Domain.CommonEntities;
using MassTransit;
using MediatR;
using JobNotFoundException = Application.Common.Exceptions.JobNotFoundException;

namespace Application.Common.Handlers.Jobs;

[Transactional]
public record CancelJobCommand(Guid JobId) : ICommand;

public class CancelJobHandler(
    IRepository<Job, Guid> repository,
    IPublishEndpoint publisher,
    IUnitOfWork unitOfWork,
    INamedObjectRegistry<LrtNamedObjectBase> registry) : ICommandHandler<CancelJobCommand>
{
    public async Task<Unit> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Job>.New()
            .Where(x => x.Id == request.JobId)
            .Track()
            .ForUpdate()
            .Build();
        
        var job = await repository.FirstOrDefaultAsync(criteria, cancellationToken)
            ?? throw new JobNotFoundException(request.JobId);

        var lrt = registry.GetBySystemName(job.SystemName);
            
        job.RequestCancellation();
        
        await publisher.Publish(
            new JobStatusUpdatedEvent
            {
                JobId = job.Id,
                Status = job.Status.ToString(),
                CurrentAttempt = job.Attempts
            },
            conf => conf.SetRoutingKey(lrt.ServiceDefinition.ServiceName),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}