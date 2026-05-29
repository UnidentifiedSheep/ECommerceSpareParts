using Abstractions;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortByOptions.Value
            .MapDefault<Metric, Guid>(x => x.Id)
            .Map<Metric, Guid>("id", x => x.Id)
            .Map<Metric, DateTime>("createdAt", x => x.CreatedAt)
            .Map<Metric, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<Metric, DateTime?>("recalculatedAt", x => x.RecalculatedAt);
    }
}