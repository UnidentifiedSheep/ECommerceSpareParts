using Abstractions.Models;
using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Projections;
using Domain.CommonEntities;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.Jobs.GetSchedule;

public record GetScheduleQuery(
    IEnumerable<string> SystemNames,
    RangeModel<DateTime>? NextRunRange,
    string? SortBy,
    Pagination Pagination) : IQuery<GetScheduleResult>;
public record GetScheduleResult(IReadOnlyList<JobScheduleDto> Schedules);

public class GetScheduleHandler(
    IReadRepository<JobSchedule, Guid> repository) : IQueryHandler<GetScheduleQuery, GetScheduleResult>
{
    public async Task<GetScheduleResult> Handle(
        GetScheduleQuery request, 
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.SystemNames.Any())
            query = query.Where(x => request.SystemNames.Contains(x.SystemName));

        if (request.NextRunRange != null && request.NextRunRange.HasBounds)
            query = query.Where(x =>
                x.NextRunAt >= request.NextRunRange.Min &&
                x.NextRunAt <= request.NextRunRange.Max);

        var result = await query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(JobProjections.JobScheduleProjection)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);
        
        return new GetScheduleResult(result);
    }
}