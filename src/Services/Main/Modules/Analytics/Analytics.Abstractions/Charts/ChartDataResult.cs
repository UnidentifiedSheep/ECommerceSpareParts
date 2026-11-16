using Main.Modules.Analytics.Abstractions.Interfaces.ChartData;

namespace Main.Modules.Analytics.Abstractions.Charts;

public sealed record ChartDataResult(IReadOnlyList<IChartDataPoint> DataPoints, string? NextCursor);

public sealed record ChartDataResult<TDataPoint>(IReadOnlyList<TDataPoint> DataPoints, string? NextCursor)
	where TDataPoint : class, IChartDataPoint
{
	public ChartDataResult ToUntyped() => new(DataPoints, NextCursor);
}
