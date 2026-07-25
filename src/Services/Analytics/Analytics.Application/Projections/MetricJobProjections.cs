using System.Linq.Expressions;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Application.Common.Interfaces.Projections;
using Attributes;

namespace Analytics.Application.Projections;

[LifetimeAttribute(Lifetime.Singleton)]
public sealed class MetricJobDtoProjectionProvider
    : IProjectionProvider<MetricJob, MetricJobDto>
{
    public Expression<Func<MetricJob, MetricJobDto>> Projection { get; } =
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
}
