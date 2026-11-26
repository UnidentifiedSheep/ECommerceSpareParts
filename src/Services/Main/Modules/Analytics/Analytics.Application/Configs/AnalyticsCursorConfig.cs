using Application.Common.Querying;
using Main.Entities.Analytics;
using Main.Modules.Analytics.Application.Charts.SalesProfitOverTime;

namespace Main.Modules.Analytics.Application.Configs;

public static class AnalyticsCursorConfig
{
	public static void Configure()
		=> QueryableCursor.Value
			.Map<SalesProfitDataPoint, DateTime>(x => x.PeriodStart)
			.Map<AnalyticsRunItem, int>(x => x.Rank);
}
