using Application.Common.Interfaces.Cqrs;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListMetrics;

public class ListMetricsCachePolicy(IScopedStringLocalizer localizer) : ICachePolicy<ListMetricsQuery>
{
    public string GetCacheKey(ListMetricsQuery request)
    {
        return $"list-metrics:{localizer.Locale}";
    }

    public TimeSpan TimeToLive => TimeSpan.FromDays(1);
    public IReadOnlyCollection<string>? Tags => null;
    public string? BaseTag => null;
}