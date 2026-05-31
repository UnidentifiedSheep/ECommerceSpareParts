using Abstractions;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Enums;

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

        QueryableSortByOptions.Value
            .MapDefault<MetricCalculationJob, Guid>(x => x.RequestId)
            .Map<MetricCalculationJob, Guid>("requestId", x => x.RequestId)
            .Map<MetricCalculationJob, DateTime>("createdAt", x => x.CreatedAt)
            .Map<MetricCalculationJob, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<MetricCalculationJob, CalculationStatus>("status", x => x.Status);
    }
}