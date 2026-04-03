using Abstractions.Models.Repository;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class MetricCalculationJobRepository(DContext context) : IMetricCalculationJobRepository
{
    public async Task<MetricCalculationJob?> GetCalculationJob(
        QueryOptions<MetricCalculationJob, Guid> options,
        CancellationToken ct = default)
    {
        return await context.MetricCalculationJobs
            .ApplyOptions(options)
            .FirstOrDefaultAsync(x => x.RequestId == options.Data, ct);
    }
}