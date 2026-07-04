using Analytics.Entities.Metrics;
using Analytics.Enums;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Interfaces.Repositories;

public interface IMetricRepository : IRepository<Metric, Guid>
{
    Task<int> MarkDirtyAsync(
        DependsOn dependsOn,
        DateTime factDatetime,
        DateTime? previousFactDatetime = null,
        CancellationToken cancellationToken = default);
}