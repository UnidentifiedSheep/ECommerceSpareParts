using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Domain.CommonEntities;
using Domain.CommonEnums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Handlers.Jobs;

public record FailExpiredJobLeasesCommand(int MaxBatchSize = 100) : ICommand;

public class FailExpiredJobLeasesHandler(
    IUnitOfWork unitOfWork,
    IRepository<Job, Guid> repository,
    ILogger<FailExpiredJobLeasesHandler> logger
) : ICommandHandler<FailExpiredJobLeasesCommand>
{
    public async Task<Unit> Handle(
        FailExpiredJobLeasesCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        List<Job> jobs = [];

        await unitOfWork.ExecuteWithTransaction(
            TransactionalAttribute.ReadCommited(30, 3),
            async () =>
            {
                var criteria = Criteria<Job>.New()
                    .Where(x =>
                        (x.Status == JobStatus.Locked || x.Status == JobStatus.Processing)
                        && x.LeaseExpiresAt != null
                        && x.LeaseExpiresAt <= now
                        && x.Attempts >= x.MaxAttempts)
                    .OrderByAsc(x => x.Id)
                    .ForUpdate(true, true)
                    .Track()
                    .Size(request.MaxBatchSize)
                    .Build();
                jobs = await repository.ListAsync(criteria, cancellationToken);

                foreach (var job in jobs)
                    job.FailByExpiredLease(
                        now, //TODO: create localization message.
                        "Job lease expired and maximum number of attempts was exceeded.");
                

                await unitOfWork.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);

        if (jobs.Count > 0)
            logger.LogWarning(
                "Jobs with expired leases are Failed. Count: {Count}",
                jobs.Count);
        

        return Unit.Value;
    }
}