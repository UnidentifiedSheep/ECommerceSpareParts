using Abstractions.Models;
using Application.Common.Dtos;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Projections;
using Domain.CommonEntities;
using LinqKit;
using Localization.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Handlers.JobSchedules.GetSchedule;

public record GetScheduleQuery(
    IEnumerable<string> JobSystemNames,
    RangeModel<DateTime>? NextRunRange,
    string? SortBy,
    Pagination Pagination
) : IQuery<GetScheduleResult>;

public record GetScheduleResult(IReadOnlyList<JobScheduleDto> Schedules);

public class GetScheduleHandler(
    IScopedStringLocalizer localizer,
    IReadRepository<JobSchedule, Guid> repository
) : IQueryHandler<GetScheduleQuery, GetScheduleResult>
{
    public async Task<GetScheduleResult> Handle(
        GetScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var query = repository.Query;

        if (request.JobSystemNames.Any())
            query = query.Where(x => request.JobSystemNames.Contains(x.JobSystemName));

        if (request.NextRunRange?.Min != null)
            query = query.Where(x => x.NextRunAt >= request.NextRunRange.Min);

        if (request.NextRunRange?.Max != null)
            query = query.Where(x => x.NextRunAt <= request.NextRunRange.Max);

        var result = await query
            .SortBy(request.SortBy)
            .AsExpandable()
            .Select(JobProjections.JobScheduleProjection(localizer))
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        return new GetScheduleResult(result);
    }
}