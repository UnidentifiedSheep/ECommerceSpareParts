using Domain.CommonEntities;

namespace Application.Common.Interfaces.Repositories;

public interface IJobRepository : IRepository<Job, Guid>
{
    Task<int> TryInsertPendingUniqueAsync(
        IEnumerable<UniqJob> jobs,
        CancellationToken cancellationToken = default);
}