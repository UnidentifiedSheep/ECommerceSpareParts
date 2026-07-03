using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.Interfaces.Repositories;
using Application.Common.NamedObject;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEnums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.Handlers.Jobs;

public record RunJobBatchCommand(int MaxBatchSize) : ICommand;

public class RunJobBatchHandler(
    IServiceScopeFactory scopeFactory,
    IUnitOfWork unitOfWork,
    ILogger<RunJobBatchHandler> logger,
    IRepository<Job, Guid> repository
) : ICommandHandler<RunJobBatchCommand>
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);

    public async Task<Unit> Handle(
        RunJobBatchCommand request,
        CancellationToken cancellationToken)
    {
        List<Job> jobs = [];
        Dictionary<Guid, Guid> leaseHolders = new();

        var now = DateTime.UtcNow;

        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                var criteria = Criteria<Job>.New()
                    .Where(x =>
                        x.Status == JobStatus.Pending ||
                        (
                            (x.Status == JobStatus.Locked || x.Status == JobStatus.Processing)
                            && x.LeaseExpiresAt != null
                            && x.LeaseExpiresAt <= now
                            && x.Attempts < x.MaxAttempts
                        ))
                    .OrderByAsc(job => job.Id)
                    .ForUpdate(true, true)
                    .Track()
                    .Size(request.MaxBatchSize)
                    .Build();
                
                jobs = await repository.ListAsync(criteria, cancellationToken);

                foreach (var job in jobs)
                {
                    var holderId = Guid.NewGuid();

                    leaseHolders.Add(job.Id, holderId);
                    job.AcquireLease(holderId, LeaseDuration);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        var tasks = jobs
            .Select(x => TakeScopeAndRun(
                leaseHolders[x.Id],
                x.Id,
                x.SystemName,
                cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);

        return Unit.Value;
    }

    private async Task TakeScopeAndRun(
        Guid holderId,
        Guid jobId,
        string systemName,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var registry = scope.ServiceProvider
                .GetRequiredService<INamedObjectRegistry<LrtNamedObjectBase>>();

            await registry
                .GetBySystemName(systemName)
                .ExecuteAsync(
                    jobId,
                    holderId,
                    cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error while executing LRT {LrtSystemName}, JobId: {JobId}",
                systemName,
                jobId);
        }
    }
}