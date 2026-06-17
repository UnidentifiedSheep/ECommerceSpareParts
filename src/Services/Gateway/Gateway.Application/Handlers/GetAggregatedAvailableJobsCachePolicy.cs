using Application.Common.Interfaces.Cqrs;

namespace Gateway.Application.Handlers;

public class GetAggregatedAvailableJobsCachePolicy : ICachePolicy<GetAggregatedAvailableJobsQuery>
{
    public TimeSpan TimeToLive => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? Tags => null;
    public string? BaseTag => null;
    public string GetCacheKey(GetAggregatedAvailableJobsQuery request)
    {
        return $"available-jobs:{request.Locale}";
    }
}