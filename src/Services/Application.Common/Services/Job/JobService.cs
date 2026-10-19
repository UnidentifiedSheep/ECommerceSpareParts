using Application.Common.Extensions;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Attributes;
using Domain.CommonEntities.Job;
using JobEntity = Domain.CommonEntities.Job.Job;

namespace Application.Common.Services.Job;

public class JobService(
	IApplicationTransaction transactionService,
	Func<IJobCreationDispatcher> jobCreationDispatcherFactory) : IJobService
{
	public Task CancelJobAsync(Guid jobId, CancellationToken token = default)
	{
		return transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 2)).ExecuteAsync(
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
				if (job is MultiStepJob multiStepJob)
				{
					var parents = new[] { multiStepJob };
					while (parents.Length > 0)
					{
						var parentIds = parents.Select(x => x.Id).ToList();
						var stepCriteria = Criteria<JobEntity>
							.New()
							.Where(x => x.MultiStepJobId.HasValue && parentIds.Contains(x.MultiStepJobId.Value))
							.Track()
							.ForUpdate()
							.Build();
						var steps = await repository.ListAsync(stepCriteria, ct);
						var parentsById = parents.ToDictionary(x => x.Id);
						foreach (var group in steps.GroupBy(x => x.MultiStepJobId))
							parentsById[group.Key!.Value].CancelUnfinishedSteps(group);

						parents = steps.OfType<MultiStepJob>().ToArray();
					}
				}

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

		return transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 3)).ExecuteAsync(
			async (ctx, ct) =>
			{
				var repository = ctx.Repositories.Get<IJobRepository>();
				return await repository.InsertJobsAsync(toAdd, ct);
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
