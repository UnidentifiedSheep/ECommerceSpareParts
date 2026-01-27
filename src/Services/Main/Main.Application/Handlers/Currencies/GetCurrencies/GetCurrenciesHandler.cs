using Application.Common.Interfaces;
using Core.Models;
using Core.StaticFunctions;
using Main.Abstractions.Dtos.Currencies;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;
using Currency = Main.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public record GetCurrenciesQuery(PaginationModel Pagination) : IQuery<GetCurrenciesResult>, ICacheableQuery
{
    public string GetCacheKey() => string.Format(CacheKeys.CurrenciesCacheKey, Pagination.Page, Pagination.Size);
    public Type GetRelatedType() => typeof(Currency);
    public int GetDurationSeconds() => 3600;
}

public record GetCurrenciesResult(IEnumerable<CurrencyDto> Currencies);

public class GetCurrenciesHandler(ICurrencyRepository currencyRepository, IRelatedDataCollector relatedDataCollector) 
    : IQueryHandler<GetCurrenciesQuery, GetCurrenciesResult>
{
    public async Task<GetCurrenciesResult> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var limit = request.Pagination.Size;
        var currencies = await currencyRepository.GetCurrencies(page, limit, false, cancellationToken);

        relatedDataCollector.AddRange(currencies.Select(x => x.Id.ToString()));

        return new GetCurrenciesResult(currencies.Adapt<List<CurrencyDto>>());
    }
}