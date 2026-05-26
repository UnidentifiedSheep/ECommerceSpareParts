using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities.Metrics;
using Analytics.Enums;
using Analytics.Persistence.Context;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Analytics.Persistence.Repositories;

public class MetricRepository(DContext context)
    : RepositoryBase<DContext, Metric, Guid>(context), IMetricRepository
{
    public async Task<int> MarkDirtyAsync(
        DependsOn dependsOn,
        DateTime factDatetime,
        DateTime? previousFactDatetime = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Metrics
            .Where(x => (x.DependsOn & dependsOn) != DependsOn.None)
            .Where(x => (x.Tags & RecalculationTags.RecalculationNeeded) == RecalculationTags.None);

        query = previousFactDatetime.HasValue
            ? query.Where(x =>
                x.RangeStart <= factDatetime && factDatetime <= x.RangeEnd ||
                x.RangeStart <= previousFactDatetime.Value && previousFactDatetime.Value <= x.RangeEnd)
            : query.Where(x => x.RangeStart <= factDatetime && factDatetime <= x.RangeEnd);

        return await query.ExecuteUpdateAsync(
            update => update.SetProperty(
                metric => metric.Tags,
                metric => metric.Tags | RecalculationTags.RecalculationNeeded),
            cancellationToken);
    }

    public override Task<Dictionary<Guid, Metric>> FindByIdsAsync(
        IEnumerable<Guid> ids,
        Criteria<Metric>? criteria = null,
        CancellationToken ct = default)
    {
        return Context.Metrics
            .Apply(criteria)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
}
