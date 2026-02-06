using Application.Common.Interfaces;
using Core.StaticFunctions;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.Handlers.Currencies.GetCurrencyRates;

public record GetCurrencyRatesQuery() : IQuery<GetCurrencyRatesResult>, ICacheableQuery
{
    public string GetCacheKey() => CacheKeys.CurrencyRatesCacheKey;
    public Type? GetRelatedType() => null;
    public int GetDurationSeconds() => 3600;
}

public record GetCurrencyRatesResult(Dictionary<int, decimal> Rates);

public class GetCurrencyRatesHandler(ICurrencyRepository currencyRepository) : IQueryHandler<GetCurrencyRatesQuery, GetCurrencyRatesResult>
{
    public async Task<GetCurrencyRatesResult> Handle(GetCurrencyRatesQuery request, CancellationToken cancellationToken)
    {
        var toUsd = await currencyRepository.GetCurrenciesToUsd(cancellationToken);
        return new GetCurrencyRatesResult(toUsd);
    }
}