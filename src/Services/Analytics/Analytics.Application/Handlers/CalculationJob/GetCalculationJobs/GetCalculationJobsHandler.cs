using Abstractions.Models;
using Analytics.Application.Dtos.CalculationJob;
using Analytics.Application.Handlers.Projections;
using Analytics.Entities;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Handlers.CalculationJob.GetCalculationJobs;

[Diagnostics(maxExecutionTimeMs: 100)]
public record GetCalculationJobsQuery(Guid MetricId, Pagination Pagination, string? SortBy) : IQuery<GetCalculationJobsResult>;
public record GetCalculationJobsResult(IReadOnlyList<CalculationJobDto> Jobs);

public class GetCalculationJobsHandler(
    IReadRepository<MetricCalculationJob, Guid> repository
    ) : IQueryHandler<GetCalculationJobsQuery, GetCalculationJobsResult>
{
    public async Task<GetCalculationJobsResult> Handle(
        GetCalculationJobsQuery request, 
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.MetricId == request.MetricId)
            .SortBy(request.SortBy)
            .ApplyPagination(request.Pagination)
            .AsExpandable()
            .Select(MetricCalculationJobProjection.ToDto)
            .ToListAsync(cancellationToken);

        return new GetCalculationJobsResult(result);
    }
}