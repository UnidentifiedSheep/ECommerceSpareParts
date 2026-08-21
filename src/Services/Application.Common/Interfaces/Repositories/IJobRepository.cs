using Domain.CommonEntities;
using Domain.CommonEntities.Job;

namespace Application.Common.Interfaces.Repositories;

public interface IJobRepository : IRepository<Job, Guid>
{
    Task<IReadOnlyList<Guid>> InsertJobsAsync(
        IEnumerable<Job> jobs,
        CancellationToken cancellationToken = default);
}
