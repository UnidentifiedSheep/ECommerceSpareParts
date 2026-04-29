using Abstractions.Models;
using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Abstractions.Constants;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public class GetCurrenciesCachePolicy(ICacheKeyRegistry keyRegistry) : ICachePolicy<GetCurrenciesQuery>
{
    public string GetCacheKey(GetCurrenciesQuery request)
        => keyRegistry.FormatKey<GetCurrenciesResult, Pagination>(request.Pagination);
    
    public int DurationSeconds => 3600;
    public Type RelatedType => typeof(Currency);
}