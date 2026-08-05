using Domain.CommonEntities;
using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Repositories;

public interface IJobRepository : IRepository<Job, Guid>
{
    Task<int> TryInsertPendingUniqueAsync(
        IEnumerable<UniqJob> jobs,
        CancellationToken cancellationToken = default);
}