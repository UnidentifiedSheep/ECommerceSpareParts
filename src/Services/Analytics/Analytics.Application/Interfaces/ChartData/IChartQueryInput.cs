using Abstractions.Models;

namespace Analytics.Application.Interfaces.ChartData;

public interface IChartQueryInput
{
}

public interface ICursorChartQueryInput<TCursor> : IChartQueryInput where TCursor : struct
{
	Cursor<TCursor?> GetCursor();
}
