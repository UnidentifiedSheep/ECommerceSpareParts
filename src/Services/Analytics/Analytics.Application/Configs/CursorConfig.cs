using Abstractions;
using Analytics.Application.NamedObjects.ChartDataSources.SalesProfitOverTime;

namespace Analytics.Application.Configs;

public static class CursorConfig
{
    public static void Configure()
    {
        QueryableCursor.Value.Map<SalesProfitDataPoint, DateTime>(x => x.PeriodStart);
    }
}
