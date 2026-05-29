using Abstractions.Models;
using Analytics.Application.Dtos.Metric;
using Analytics.Application.Handlers.Metrics.ListMetrics;
using Analytics.Application.Handlers.Projections;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.Metrics.GetMetrics;

public record GetMetricsQuery(string? SortBy, Pagination Pagination) : IQuery<GetMetricsResult>;
public record GetMetricsResult(IReadOnlyList<MetricDto> Metrics);

public class GetMetricsHandler(
    IReadRepository<Metric, Guid> metricRepository,
    ISender sender) : IQueryHandler<GetMetricsQuery, GetMetricsResult>
{
    public async Task<GetMetricsResult> Handle(GetMetricsQuery request, CancellationToken cancellationToken)
    {
        var metrics = await metricRepository
            .Query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(MetricProjection.ToDto(await GetMetricInfos(cancellationToken)))
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetMetricsResult(metrics);
    }

    private async Task<IReadOnlyDictionary<string, MetricInfoDto>> GetMetricInfos(CancellationToken cancellationToken)
    {
        return (await sender.Send(new ListAvailableMetricsQuery(), cancellationToken))
            .Metrics
            .ToDictionary(x => x.SystemName);
    }
}