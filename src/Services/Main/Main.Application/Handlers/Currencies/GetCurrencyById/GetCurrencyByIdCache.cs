using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public class GetCurrencyByIdCache(ICacheKey<GetCurrencyByIdResult> cacheKey) : ICachePolicy<GetCurrencyByIdQuery>
{
    public string GetCacheKey(GetCurrencyByIdQuery request) => cacheKey.FormatKey(request.Id);
    public int DurationSeconds => 6000;
    public Type RelatedType => typeof(Currency);
}

public sealed class GetCurrencyByIdCacheKey : CacheKeyBase<GetCurrencyByIdResult>
{
    public override string KeyTemplate => "currency:{0}";
}