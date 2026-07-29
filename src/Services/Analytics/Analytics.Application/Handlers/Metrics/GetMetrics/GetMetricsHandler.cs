using Abstractions.Models;
using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.Metrics.GetMetrics;

[Diagnostics(maxExecutionTimeMs: 200)]
public record GetMetricsQuery(
    string? MetricSystemName,
    string[] SortBy,
    Pagination Pagination
) : IQuery<GetMetricsResult>;

public record GetMetricsResult(IReadOnlyList<MetricDto> Metrics);

public class GetMetricsHandler(
    IReadRepository<Metric, Guid> metricRepository,
    IProjectionProvider<Metric, MetricDto> projection
) : IQueryHandler<GetMetricsQuery, GetMetricsResult>
{
    public async Task<GetMetricsResult> Handle(
        GetMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var query = metricRepository.Query;

        if (!string.IsNullOrWhiteSpace(request.MetricSystemName))
            query = query.Where(x => x.Discriminator == request.MetricSystemName);

        var metrics = await query
            .SortBy(request.SortBy)
            .Project(projection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetMetricsResult(metrics);
    }
}
