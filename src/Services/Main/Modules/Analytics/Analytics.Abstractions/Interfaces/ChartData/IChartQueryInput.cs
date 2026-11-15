using Application.Common.Models;

namespace Main.Application.Analytics.Interfaces.ChartData;

public interface IChartQueryInput
{
}

public interface ICursorChartQueryInput<TCursor> : IChartQueryInput where TCursor : struct
{
	Cursor<TCursor?> GetCursor();
}
