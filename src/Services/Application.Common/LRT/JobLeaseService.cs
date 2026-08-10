using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;

namespace Application.Common.LRT;

public class JobLeaseService(
    IApplicationTransactionService transactionService) : IJobLeaseService
{
    public async Task<Job?> TryAcquireJobAsync(
        Guid holderId,
        TimeSpan leaseDuration,
        CancellationToken ct)
        => await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                var repository = context.Repositories.Get<Job, Guid>();
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
                
                var job = await repository.FirstOrDefaultAsync(criteria, cancellationToken);

                if (job == null) return null;

                job.AcquireLease(holderId, leaseDuration);
                
                await context.UnitOfWork.SaveChangesAsync(cancellationToken);
                return job;
            },
            ct);
    
    public async Task<List<Job>> FailExpiredJobsWithoutAttempts(int maxBatchSize, CancellationToken ct)
        => await transactionService.ExecuteAsync(
            TransactionalAttribute.ReadCommitted(30, 3),
            async (context, cancellationToken) =>
            {
                var repository = context.Repositories.Get<Job, Guid>();
                var now = DateTime.UtcNow;
                var criteria = GetCriteriaBase(maxBatchSize)
                    .Where(x =>
                        (x.Status == JobStatus.Locked || x.Status == JobStatus.Processing)
                        && x.LeaseExpiresAt != null
                        && x.LeaseExpiresAt <= now
                        && x.Attempts >= x.MaxAttempts)
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
    
    private static CriteriaBuilder<Job> GetCriteriaBase(int maxBatchSize)
        => Criteria<Job>.New()
            .OrderByAsc(job => job.Id)
            .ForUpdate(true, true)
            .Track()
            .Size(maxBatchSize);
}
