using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Dtos.Runs;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Analytics.Application.Queries;

public sealed record GetAnalyticsRunsQuery(
	string? AnalysisType,
	bool? IsPinned,
	DateTime? PeriodFrom,
	DateTime? PeriodTo,
	Cursor<(Guid Id, DateTime CreatedAt)> Cursor) : IQuery<GetAnalyticsRunsResult>;

public sealed record GetAnalyticsRunsResult(IReadOnlyList<AnalyticsRunDto> Runs);

public sealed class GetAnalyticsRunsHandler(
	IReadRepository<AnalyticsRun, Guid> repository,
	IProjectionProvider<AnalyticsRun, AnalyticsRunDto> projection)
	: IQueryHandler<GetAnalyticsRunsQuery, GetAnalyticsRunsResult>
{
	public async Task<GetAnalyticsRunsResult> Handle(
		GetAnalyticsRunsQuery request,
		CancellationToken cancellationToken)
	{
		var cursor = request.Cursor;
		var query = repository.Query;

		if (!string.IsNullOrWhiteSpace(request.AnalysisType))
		{
			var analysisType = request.AnalysisType.Trim();
			query = query.Where(run => run.AnalysisType == analysisType);
		}

		if (request.IsPinned is { } isPinned)
			query = query.Where(run => run.IsPinned == isPinned);

		if (request.PeriodFrom is { } periodFrom)
			query = query.Where(run => run.PeriodEnd == null || run.PeriodEnd > periodFrom);

		if (request.PeriodTo is { } periodTo)
			query = query.Where(run => run.PeriodStart == null || run.PeriodStart < periodTo);

		if (cursor.CursorValue != (Guid.Empty, DateTime.MinValue))
		{
			var (cursorId, cursorCreatedAt) = cursor.CursorValue;
			query = query.Where(run =>
				run.CreatedAt < cursorCreatedAt || run.CreatedAt == cursorCreatedAt && run.Id < cursorId);
		}

		var runs = await query
			.OrderByDescending(run => run.CreatedAt)
			.ThenByDescending(run => run.Id)
			.Take(cursor.Size)
			.Project(projection)
			.ToListAsync(cancellationToken);

		return new GetAnalyticsRunsResult(runs);
	}
}
