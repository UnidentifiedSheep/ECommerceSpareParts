using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using LinqKit;

namespace Analytics.Application.Handlers.Projections;

public static class MetricJobProjection
{
    public static readonly Expression<Func<MetricJob, MetricJobDto>> ToDto =
        x => new MetricJobDto
        {
            JobId = x.JobId,
            MetricId = x.MetricId,
            UpdatedAt = x.Job.UpdatedAt,
            CreatedAt = x.Job.CreatedAt,
            Status = x.Job.Status,
            ErrorMessage = x.Job.ErrorMessage,
            Attempts = x.Job.Attempts,
            MaxAttempts = x.Job.MaxAttempts
        };

    public static readonly Expression<Func<MetricJob?, MetricJobDto?>> ToDtoOrDefault =
        x => x == null
            ? null
            : ToDto.Invoke(x);
}