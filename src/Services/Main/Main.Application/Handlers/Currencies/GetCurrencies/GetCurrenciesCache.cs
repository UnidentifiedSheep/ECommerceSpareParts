using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Abstractions.Constants;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesCache(ICacheKey<GetCurrenciesResult> cacheKey) : ICachePolicy<GetCurrenciesQuery>
{
    public string GetCacheKey(GetCurrenciesQuery request)
        => cacheKey.FormatKey(request.Pagination.Page, request.Pagination.Size);
    
    public int DurationSeconds => 3600;
    public Type RelatedType => typeof(Currency);
}

public class CurrencyCacheKey : CacheKeyBase<GetCurrenciesResult>
{
    public override string KeyTemplate => "currencies:{0}-{1}";
}