using Analytics.Application.Dtos.Metric;
using Analytics.Entities.Exceptions;
using Analytics.Entities.Metrics;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.Metrics;

public record GetMetricQuery(Guid MetricId) : IQuery<GetMetricResult>;

public record GetMetricResult(MetricDto Metric);

public class GetMetricHandler(
    IReadRepository<Metric, Guid> repository,
    IProjectionProvider<Metric, MetricDto> projection)
    : IQueryHandler<GetMetricQuery, GetMetricResult>
{
    public async Task<GetMetricResult> Handle(
        GetMetricQuery request,
        CancellationToken cancellationToken)
    {
        var metric = await repository.Query
            .Where(x => x.Id == request.MetricId)
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new MetricNotFoundException(request.MetricId);

        return new GetMetricResult(metric);
    }
}
