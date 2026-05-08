using Analytics.Abstractions.Consts;
using Application.Common.Interfaces;
using Localization.Abstractions.Interfaces;

namespace Analytics.Application.Handlers.Metrics.ListMetrics;

public class ListMetricsCachePolicy(IScopedStringLocalizer localizer) : ICachePolicy<ListMetricsQuery>
{
    public int DurationSeconds => 60000;
    public Type? RelatedType => null;

    public string GetCacheKey(ListMetricsQuery request)
    {
        return string.Format(CacheKeys.ListMetricsCacheKey, localizer.Locale);
    }
}