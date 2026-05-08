using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities.Metrics;
using Analytics.Persistence.Context;

namespace Analytics.Persistence.Repositories;

public class MetricRepository(DContext context) : IMetricRepository
{
    public async Task<Metric?> GetMetric(
        QueryOptions<Metric, Guid> options,
        CancellationToken cancellationToken = default)
    {
        return await context.Metrics
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.Id == options.Data, cancellationToken);
    }
}