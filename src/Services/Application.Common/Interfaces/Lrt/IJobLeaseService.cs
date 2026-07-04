using Domain.CommonEntities;

namespace Application.Common.Interfaces.Lrt;

public interface IJobLeaseService
{
    Task<Job?> TryAcquireJobAsync(
        Guid holderId,
        TimeSpan leaseDuration,
        CancellationToken ct);

    Task<List<Job>> FailExpiredJobsWithoutAttempts(
        int maxBatchSize,
        CancellationToken ct);
}