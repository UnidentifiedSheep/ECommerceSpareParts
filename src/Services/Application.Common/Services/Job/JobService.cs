using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Attributes;
using JobEntity = Domain.CommonEntities.Job.Job;

namespace Application.Common.Services.Job;

public class JobService(
	IApplicationTransactionService transactionService,
	Func<IJobCreationDispatcher> jobCreationDispatcherFactory) : IJobService
{
	public Task CancelJobAsync(Guid jobId, CancellationToken token = default)
	{
		return transactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 2),
			async (ctx, ct) =>
			{
				var repository = ctx.Repositories.Get<IJobRepository>();
				var criteria = Criteria<JobEntity>
					.New()
					.Where(x => x.Id == jobId)
					.Track()
					.ForUpdate()
					.Build();

				var job = await repository.FirstOrDefaultAsync(criteria, ct) ??
					throw new JobNotFoundException(jobId);

				job.RequestCancellation();

				await ctx.UnitOfWork.SaveChangesAsync(ct);
			},
			token);
	}

	public Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
		IEnumerable<JobEntity> jobs,
		CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(jobs);
		var toAdd = jobs.ToList();

		return transactionService.ExecuteAsync(
			TransactionalAttribute.ReadCommitted(30, 3),
			async (ctx, ct) =>
			{
				var repository = ctx.Repositories.Get<IJobRepository>();
				var addedIds = await repository.InsertJobsAsync(toAdd, ct);

				if (addedIds.Count != 0)
					await ctx.UnitOfWork.SaveChangesAsync(ct);

				return addedIds;
			},
			token);
	}

	public Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
		IEnumerable<IJobItem> jobs,
		CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(jobs);

		var jobCreationDispatcher = jobCreationDispatcherFactory();
		var toAdd = new List<JobEntity>();
		foreach (var item in jobs)
			toAdd.Add(
				jobCreationDispatcher.Create(
					item.SystemName,
					item.InputState,
					item.MaxAttempts,
					item.NaturalKey));

		return TryEnqueueJobsAsync(toAdd, token);
	}
}
