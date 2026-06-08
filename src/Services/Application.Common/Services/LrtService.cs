using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Attributes;
using Contracts.Lrt;
using Domain.CommonEntities;
using MassTransit;

namespace Application.Common.Services;

public class LrtService(
    IRepository<Job, Guid> repository,
    INamedObjectRegistry<LrtNamedObjectBase> lrtRegistry,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork
    ) : ILrtService
{
    public async Task RunLrtAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await repository.GetById(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job with id {jobId} not found");
        var lrt = lrtRegistry.GetBySystemName(job.SystemName);
        await lrt.ExecuteAsync(jobId, cancellationToken);
    }

    public async Task QueueJob(
        Job job,
        CancellationToken cancellationToken = default)
    {
        _ = lrtRegistry.TryGetBySystemName(job.SystemName)
            ?? throw new InvalidOperationException($"Unable to queue job, invalid system name {job.SystemName}");

        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(20, 3),
            async () =>
            {
                await unitOfWork.AddAsync(job, cancellationToken);
                await publishEndpoint.Publish(new JobQueuedEvent
                {
                    JobId = job.Id,
                }, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);
    }
}