using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Dtos.Runs;

namespace Main.Modules.Analytics.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class AnalyticsRunDtoProjectionProvider : ProjectionProviderBase<AnalyticsRun, AnalyticsRunDto>
{
	public override Expression<Func<AnalyticsRun, AnalyticsRunDto>> Projection { get; } = run => new AnalyticsRunDto
	{
		Id = run.Id,
		AnalysisType = run.AnalysisType,
		SchemaVersion = run.SchemaVersion,
		Parameters = run.Parameters,
		Summary = run.Summary,
		ItemsCount = run.ItemsCount,
		PeriodStart = run.PeriodStart,
		PeriodEnd = run.PeriodEnd,
		JobId = run.JobId,
		CreatedBy = run.CreatedBy,
		IsPinned = run.IsPinned,
		CreatedAt = run.CreatedAt,
		CompletedAt = run.CompletedAt
	};
}
