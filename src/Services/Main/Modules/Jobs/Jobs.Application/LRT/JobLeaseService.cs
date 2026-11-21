using System.Linq.Expressions;
using Application.Common.Extensions;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Jobs.Application.LRT;

public class JobLeaseService(
	IApplicationTransaction transactionService,
	IReadRepository<Job, Guid> readRepository) : IJobLeaseService
{
	public async Task<Job?> TryAcquireJobAsync(
		Guid holderId,
		TimeSpan leaseDuration,
		CancellationToken ct)
		=> (await AcquireAsync(
				null,
				[holderId],
				leaseDuration,
				ct))
			.SingleOrDefault();

	public async Task<IReadOnlyList<Job>> AcquireJobsAsync(
		IReadOnlyCollection<string> systemNames,
		IReadOnlyList<Guid> holderIds,
		TimeSpan leaseDuration,
		CancellationToken ct)
	{
		if (systemNames.Count == 0 || holderIds.Count == 0)
			return [];

		return await AcquireAsync(
			systemNames,
			holderIds,
			leaseDuration,
			ct);
	}

	public async Task<IReadOnlyList<string>> GetAcquirableSystemNamesAsync(
		IReadOnlyCollection<string> systemNames,
		CancellationToken ct)
	{
		if (systemNames.Count == 0)
			return [];

		return await readRepository.Query
			.Where(IsAcquirable(DateTime.UtcNow, systemNames.ToArray()))
			.Select(x => x.SystemName)
			.Distinct()
			.ToListAsync(ct);
	}

	public async Task<List<Job>> FailExpiredJobsWithoutAttempts(int maxBatchSize, CancellationToken ct)
		=> await transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 3))
			.ExecuteAsync(
				async (context, cancellationToken) =>
				{
					var repository = context.Repositories.Get<Job, Guid>();
					var now = DateTime.UtcNow;
					var criteria = GetCriteriaBase(maxBatchSize)
						.Where(x =>
							(x.Status == JobStatus.Locked || x.Status == JobStatus.Processing) &&
							x.LeaseExpiresAt != null && x.LeaseExpiresAt <= now &&
							x.Attempts >= x.MaxAttempts)
						.Build();
					var jobs = await repository.ListAsync(criteria, cancellationToken);

					foreach (var job in jobs)
						job.FailByExpiredLease(
							now, //TODO: create localization message.
							"Job lease expired and maximum number of attempts was exceeded.");

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
					return jobs;
				},
				ct);

	public async Task<List<Job>> CancelExpiredJobs(int maxBatchSize, CancellationToken ct)
		=> await transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 3))
			.ExecuteAsync(
				async (context, cancellationToken) =>
				{
					var repository = context.Repositories.Get<Job, Guid>();
					var now = DateTime.UtcNow;
					var criteria = GetCriteriaBase(maxBatchSize)
						.Where(x => x.Status == JobStatus.CancellationRequested &&
							x.LeaseExpiresAt != null && x.LeaseExpiresAt <= now)
						.Build();
					var jobs = await repository.ListAsync(criteria, cancellationToken);

					foreach (var job in jobs)
						job.CancelByExpiredLease(now);

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
					return jobs;
				},
				ct);

	private async Task<List<Job>> AcquireAsync(
		IReadOnlyCollection<string>? systemNames,
		IReadOnlyList<Guid> holderIds,
		TimeSpan leaseDuration,
		CancellationToken ct)
		=> await transactionService.WithSettingsIfIdle(TransactionalAttribute.ReadCommitted(30, 3))
			.ExecuteAsync(
				async (context, cancellationToken) =>
				{
					var repository = context.Repositories.Get<Job, Guid>();
					var criteria = GetCriteriaBase(holderIds.Count)
						.Where(IsAcquirable(DateTime.UtcNow, systemNames?.ToArray()))
						.Build();

					var jobs = await repository.ListAsync(criteria, cancellationToken);
					if (jobs.Count == 0)
						return jobs;

					for (var i = 0; i < jobs.Count; i++)
						jobs[i].AcquireLease(holderIds[i], leaseDuration);

					await context.UnitOfWork.SaveChangesAsync(cancellationToken);
					return jobs;
				},
				ct);

	private static Expression<Func<Job, bool>> IsAcquirable(DateTime now, IEnumerable<string>? names)
		=> x =>
			(x.Status == JobStatus.Pending ||
				(x.Status == JobStatus.Locked || x.Status == JobStatus.Processing) &&
				x.LeaseExpiresAt != null && x.LeaseExpiresAt <= now &&
				x.Attempts < x.MaxAttempts) &&
			(names == null || names.Contains(x.SystemName));

	private static CriteriaBuilder<Job> GetCriteriaBase(int maxBatchSize)
		=> Criteria<Job>
			.New()
			.OrderByAsc(x => x.CreatedAt)
			.ThenByAsc(job => job.Id)
			.ForUpdate(true, true)
			.Track()
			.Size(maxBatchSize);
}
