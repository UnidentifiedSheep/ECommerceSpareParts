using Application.Common.Abstractions;
using Application.Common.Interfaces;
using Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencyById;

public class GetCurrencyByIdCachePolicy(ICacheKeyRegistry keyRegistry) : ICachePolicy<GetCurrencyByIdQuery>
{
    public string GetCacheKey(GetCurrencyByIdQuery request) 
        => keyRegistry.FormatKey<GetCurrencyByIdResult, int>(request.Id);
    public int DurationSeconds => 6000;
    public Type RelatedType => typeof(Currency);
}