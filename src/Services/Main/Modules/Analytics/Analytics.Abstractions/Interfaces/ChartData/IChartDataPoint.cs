using Main.Modules.Analytics.Abstractions.Models;

namespace Main.Modules.Analytics.Abstractions.Interfaces.ChartData;

public interface IChartDataPoint
{
	IReadOnlyList<ChartReference> GetReferences() => [];
}
