using Abstractions.Models.Repository;
using Analytics.Entities.Metrics;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface IMetricRepository
{
    Task<Metric?> GetMetric(
        QueryOptions<Metric, Guid> options, 
        CancellationToken cancellationToken = default);
}