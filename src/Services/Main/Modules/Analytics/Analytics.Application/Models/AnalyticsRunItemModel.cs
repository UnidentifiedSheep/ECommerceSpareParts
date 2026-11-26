using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Services;

namespace Main.Modules.Analytics.Application.Models;

public sealed record AnalyticsRunItemModel<TSegments, TData>(
	string SubjectKey,
	int Rank,
	decimal? Value,
	decimal? Share,
	TSegments Segments,
	TData Data)
	where TSegments : notnull
	where TData : notnull
{
	public AnalyticsRunItem ToEntity(Guid runId)
		=> AnalyticsRunItem.Create(
			runId,
			SubjectKey,
			Rank,
			Value,
			Share,
			AnalyticsJson.Serialize(Segments),
			AnalyticsJson.Serialize(Data));

	public static AnalyticsRunItemModel<TSegments, TData> From(AnalyticsRunItem item)
		=> new(
			item.SubjectKey,
			item.Rank,
			item.Value,
			item.Share,
			AnalyticsJson.Deserialize<TSegments>(item.Segments),
			AnalyticsJson.Deserialize<TData>(item.Data));
}
