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

namespace Application.Common.Handlers.Jobs;

public record RunJobBatchCommand(int MaxBatchSize) : ICommand;

public class RunJobBatchHandler(
    IServiceScopeFactory scopeFactory,
    IUnitOfWork unitOfWork,
    IRepository<Job, Guid> repository
) : ICommandHandler<RunJobBatchCommand>
{
    public async Task<Unit> Handle(RunJobBatchCommand request, CancellationToken cancellationToken)
    {
        List<Job> jobs = [];
        Dictionary<Guid, Guid> locks = new();
        
        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                jobs = await repository
                    .ListAsync(
                        Criteria<Job>.New()
                            .Where(x => x.Status == JobStatus.Pending)
                            .OrderByAsc(job => job.Id)
                            .ForUpdate(true, true)
                            .Track()
                            .Size(request.MaxBatchSize)
                            .Build(),
                        cancellationToken);

                jobs.ForEach(x =>
                {
                    var holderId = Guid.NewGuid();
                    locks.Add(x.Id, holderId);
                    x.Lock(holderId, TimeSpan.FromMinutes(5));
                });
                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        var tasks = jobs
            .Select(x => TakeScopeAndRun(
                locks,
                x.Id,
                x.SystemName,
                cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);

        return Unit.Value;
    }

    private async Task TakeScopeAndRun(
        Dictionary<Guid, Guid> locks,
        Guid jobId,
        string systemName,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var registry = scope.ServiceProvider
            .GetRequiredService<INamedObjectRegistry<LrtNamedObjectBase>>();

        await registry
            .GetBySystemName(systemName)
            .ExecuteAsync(jobId, locks[jobId], cancellationToken);
    }
}