using Abstractions.Models.Repository;
using Analytics.Entities;

namespace Analytics.Abstractions.Interfaces.DbRepositories;

public interface IMetricCalculationJobRepository
{
    Task<MetricCalculationJob?> GetCalculationJob(
        QueryOptions<MetricCalculationJob, Guid> options,
        CancellationToken ct = default);
}