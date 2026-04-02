using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.Currencies.GetCurrencyRates;

public class GetCurrencyRatesCachePolicy : ICachePolicy<GetCurrencyRatesQuery>
{
    public string GetCacheKey(GetCurrencyRatesQuery request)
        => CacheKeys.CurrencyRatesCacheKey;

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}