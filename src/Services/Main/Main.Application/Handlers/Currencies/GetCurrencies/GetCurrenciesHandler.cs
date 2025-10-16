using Application.Common.Interfaces;
using Core.Models;
using Core.StaticFunctions;
using Main.Core.Dtos.Currencies;
using Main.Core.Entities;
using Main.Core.Interfaces.DbRepositories;
using Mapster;
using Currency = Main.Core.Entities.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public record GetCurrenciesQuery(PaginationModel Pagination) : IQuery<GetCurrenciesResult>, ICacheableQuery
{
    public HashSet<string> RelatedEntityIds { get; } = [];

    public string GetCacheKey()
    {
        return string.Format(CacheKeys.CurrenciesCacheKey, Pagination.Page, Pagination.Size);
    }

    public Type GetRelatedType()
    {
        return typeof(Currency);
    }

    public int GetDurationSeconds()
    {
        return 3600;
    }
}

public record GetCurrenciesResult(IEnumerable<CurrencyDto> Currencies);

public class GetCurrenciesHandler(ICurrencyRepository currencyRepository)
    : IQueryHandler<GetCurrenciesQuery, GetCurrenciesResult>
{
    public async Task<GetCurrenciesResult> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var limit = request.Pagination.Size;
        var currencies = await currencyRepository.GetCurrencies(page, limit, false, cancellationToken);

        request.RelatedEntityIds.UnionWith(currencies.Select(x => x.Id.ToString()));

        return new GetCurrenciesResult(currencies.Adapt<List<CurrencyDto>>());
    }
}