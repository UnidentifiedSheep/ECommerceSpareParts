using Abstractions;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Domain.CommonEnums;

namespace Analytics.Application.Configs;

public static class SortByConfig
{
    public static void Configure()
    {
        QueryableSortBy.Value
            .MapDefault<Metric, Guid>(x => x.Id)
            .Map<Metric, Guid>("id", x => x.Id)
            .Map<Metric, DateTime>("createdAt", x => x.CreatedAt)
            .Map<Metric, DateTime>("updatedAt", x => x.UpdatedAt)
            .Map<Metric, DateTime?>("recalculatedAt", x => x.RecalculatedAt);

        QueryableSortBy.Value
            .MapDefault<MetricJob, DateTime>(x => x.Job.CreatedAt, desc: true)
            .Map<MetricJob, Guid>("jobId", x => x.JobId)
            .Map<MetricJob, Guid>("metricId", x => x.MetricId)
            .Map<MetricJob, DateTime>("createdAt", x => x.Job.CreatedAt)
            .Map<MetricJob, DateTime>("updatedAt", x => x.Job.UpdatedAt)
            .Map<MetricJob, JobStatus>("status", x => x.Job.Status);
        
        QueryableSortBy.Value.ConfigureForJob();
    }
}
