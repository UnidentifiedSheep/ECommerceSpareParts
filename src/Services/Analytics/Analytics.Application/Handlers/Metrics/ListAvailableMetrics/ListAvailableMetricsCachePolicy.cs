using Application.Common.Interfaces.Cqrs;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListMetrics;

public class ListAvailableMetricsCachePolicy(IScopedStringLocalizer localizer) : ICachePolicy<ListAvailableMetricsQuery>
{
    public string GetCacheKey(ListAvailableMetricsQuery request)
    {
        return $"list-metrics:{localizer.Locale}";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string>? Tags => null;
    public string? BaseTag => null;
}