using Application.Common.Models;
using JobEntity = Domain.CommonEntities.Job.Job;

namespace Application.Common.Interfaces.Services;

public interface IJobService
{
	Task CancelJobAsync(Guid jobId, CancellationToken token = default);

	Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
		IEnumerable<JobEntity> jobs,
		CancellationToken token = default);

	Task<IReadOnlyList<Guid>> TryEnqueueJobsAsync(
		IEnumerable<IJobItem> jobs,
		CancellationToken token = default);
}
