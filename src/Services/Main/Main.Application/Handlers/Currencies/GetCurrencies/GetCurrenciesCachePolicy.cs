using Application.Common.Interfaces;
using Main.Abstractions.Constants;
using Main.Entities;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesCachePolicy : ICachePolicy<GetCurrenciesQuery>
{
    public string GetCacheKey(GetCurrenciesQuery request)
        => string.Format(CacheKeys.CurrenciesCacheKey, request.Pagination.Page, request.Pagination.Size);

    public int DurationSeconds => 3600;
    public Type? RelatedType => typeof(Currency);
}