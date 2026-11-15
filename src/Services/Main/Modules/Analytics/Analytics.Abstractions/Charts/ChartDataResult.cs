using Main.Application.Analytics.Interfaces.ChartData;

namespace Main.Application.Analytics.Charts;

public sealed record ChartDataResult(IReadOnlyList<IChartDataPoint> DataPoints, string? NextCursor);

public sealed record ChartDataResult<TDataPoint>(IReadOnlyList<TDataPoint> DataPoints, string? NextCursor)
	where TDataPoint : class, IChartDataPoint
{
	public ChartDataResult ToUntyped() => new(DataPoints, NextCursor);
}
