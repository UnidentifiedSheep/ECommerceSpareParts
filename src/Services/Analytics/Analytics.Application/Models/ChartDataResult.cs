using Analytics.Application.Interfaces.ChartData;

namespace Analytics.Application.Models;

public sealed record ChartDataResult(IReadOnlyList<IChartDataPoint> DataPoints, string? NextCursor);

public sealed record ChartDataResult<TDataPoint>(IReadOnlyList<TDataPoint> DataPoints, string? NextCursor)
	where TDataPoint : class, IChartDataPoint
{
	public ChartDataResult ToUntyped()
	{
		return new ChartDataResult(DataPoints, NextCursor);
	}
}
