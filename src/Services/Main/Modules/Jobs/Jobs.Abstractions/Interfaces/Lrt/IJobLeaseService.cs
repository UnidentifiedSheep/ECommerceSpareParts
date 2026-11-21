using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Lrt;

public interface IJobLeaseService
{
	Task<Job?> TryAcquireJobAsync(
		Guid holderId,
		TimeSpan leaseDuration,
		CancellationToken ct);

	Task<IReadOnlyList<Job>> AcquireJobsAsync(
		IReadOnlyCollection<string> systemNames,
		IReadOnlyList<Guid> holderIds,
		TimeSpan leaseDuration,
		CancellationToken ct);

	Task<IReadOnlyList<string>> GetAcquirableSystemNamesAsync(
		IReadOnlyCollection<string> systemNames,
		CancellationToken ct);

	Task<List<Job>> FailExpiredJobsWithoutAttempts(int maxBatchSize, CancellationToken ct);

	Task<List<Job>> CancelExpiredJobs(int maxBatchSize, CancellationToken ct);
}
