using Abstractions.Models;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.Metrics;

[Diagnostics(maxExecutionTimeMs: 200)]
public record GetMetricJobsQuery(
    Guid MetricId,
    Pagination Pagination,
    string? SortBy
) : IQuery<GetMetricJobsResult>;

public record GetMetricJobsResult(IReadOnlyList<MetricJobDto> Jobs);

public class GetMetricJobsHandler(
    IReadRepository<MetricJob, (Guid, Guid)> repository,
    IProjectionProvider<MetricJob, MetricJobDto> projection
)
    : IQueryHandler<GetMetricJobsQuery, GetMetricJobsResult>
{
    public async Task<GetMetricJobsResult> Handle(
        GetMetricJobsQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await repository.Query
            .Where(x => x.MetricId == request.MetricId)
            .SortBy(request.SortBy)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetMetricJobsResult(jobs);
    }
}
