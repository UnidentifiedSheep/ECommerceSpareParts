using Application.Common.Interfaces;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesCachePolicy : ICachePolicy<GetCurrenciesQuery>
{
    public string GetCacheKey(GetCurrenciesQuery request)
        => $"currencies:{request.Pagination.Page}-{request.Pagination.Size}";

    public TimeSpan TimeToLive => TimeSpan.FromDays(3);

    public IReadOnlyCollection<string> Tags => ["currency"];
}