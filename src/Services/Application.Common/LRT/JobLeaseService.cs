using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEnums;

namespace Application.Common.LRT;

public class JobLeaseService(
    IRepository<Job, Guid> repository,
    IUnitOfWork unitOfWork) : IJobLeaseService
{
    public async Task<Job?> TryAcquireJobAsync(
        Guid holderId,
        TimeSpan leaseDuration,
        CancellationToken ct)
        => await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                var now = DateTime.UtcNow;
                var criteria = GetCriteriaBase(1)
                    .Where(x =>
                        x.Status == JobStatus.Pending ||
                        (
                            (x.Status == JobStatus.Locked || x.Status == JobStatus.Processing)
                            && x.LeaseExpiresAt != null
                            && x.LeaseExpiresAt <= now
                            && x.Attempts < x.MaxAttempts
                        ))
                    .Build();
                
                var job = await repository.FirstOrDefaultAsync(criteria, ct);

                if (job == null) return null;

                job.AcquireLease(holderId, leaseDuration);
                
                await unitOfWork.SaveChangesAsync(ct);
                return job;
            },
            ct);
    
    public async Task<List<Job>> FailExpiredJobsWithoutAttempts(int maxBatchSize, CancellationToken ct)
        => await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                var now = DateTime.UtcNow;
                var criteria = GetCriteriaBase(maxBatchSize)
                    .Where(x =>
                        (x.Status == JobStatus.Locked || x.Status == JobStatus.Processing)
                        && x.LeaseExpiresAt != null
                        && x.LeaseExpiresAt <= now
                        && x.Attempts >= x.MaxAttempts)
                    .Build();
                var jobs = await repository.ListAsync(criteria, ct);

                foreach (var job in jobs)
                    job.FailByExpiredLease(
                        now, //TODO: create localization message.
                        "Job lease expired and maximum number of attempts was exceeded.");
                
                await unitOfWork.SaveChangesAsync(ct);
                return jobs;
            },
            ct);
    
    private static CriteriaBuilder<Job> GetCriteriaBase(int maxBatchSize)
        => Criteria<Job>.New()
            .OrderByAsc(job => job.Id)
            .ForUpdate(true, true)
            .Track()
            .Size(maxBatchSize);
}