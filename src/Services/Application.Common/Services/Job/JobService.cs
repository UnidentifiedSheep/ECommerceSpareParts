using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Attributes;
using Microsoft.Extensions.DependencyInjection;
using JobEntity = Domain.CommonEntities.Job.Job;

namespace Application.Common.Services.Job;

public class JobService(
    IApplicationTransactionService transactionService,
    IServiceProvider serviceProvider) : IJobService
{
    public Task CancelJobAsync(
        Guid jobId,
        CancellationToken token = default)
    {
        return transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 2),
            action: async (ctx, ct) =>
            {
                var repository = ctx.Repositories.Get<IJobRepository>();
                var criteria = Criteria<JobEntity>
                    .New()
                    .Where(x => x.Id == jobId)
                    .Track()
                    .ForUpdate()
                    .Build();

                var job = await repository.FirstOrDefaultAsync(criteria, ct)
                          ?? throw new JobNotFoundException(jobId);

                job.RequestCancellation();

                await ctx.UnitOfWork.SaveChangesAsync(ct);
            },
            cancellationToken: token);
    }

    public Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
        IEnumerable<JobEntity> jobs,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(jobs);
        var toAdd = jobs.ToList();

        return transactionService.ExecuteAsync(
            settings: TransactionalAttribute.ReadCommitted(30, 3),
            action: async (ctx, ct) =>
            {
                var repository = ctx.Repositories.Get<IJobRepository>();
                var addedIds = await repository.InsertJobsAsync(
                    toAdd,
                    ct);

                if (addedIds.Count != 0)
                    await ctx.UnitOfWork.SaveChangesAsync(ct);

                return addedIds;
            },
            cancellationToken: token);
    }
    
    public Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
        IEnumerable<IJobItem> jobs,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(jobs);

        var jobCreationDispatcher = serviceProvider
            .GetRequiredService<IJobCreationDispatcher>();
        var toAdd = new List<JobEntity>();
        foreach (var item in jobs)
            toAdd.Add(jobCreationDispatcher.Create(
                item.SystemName,
                item.InputState,
                item.MaxAttempts,
                item is UniqJobItem uniqJobItem
                    ? uniqJobItem.NaturalKey
                    : null));
        
        return TryEnqueueJobsAsync(toAdd, token);
    }
}
