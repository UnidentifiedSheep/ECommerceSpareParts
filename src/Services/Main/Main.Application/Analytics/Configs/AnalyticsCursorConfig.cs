using Application.Common.Querying;
using Main.Application.Analytics.Charts.SalesProfitOverTime;

namespace Main.Application.Analytics.Configs;

public static class AnalyticsCursorConfig
{
	public static void Configure() =>
		QueryableCursor.Value.Map<SalesProfitDataPoint, DateTime>(x => x.PeriodStart);
}
