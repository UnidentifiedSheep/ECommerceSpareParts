using Abstractions.Models;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Currencies;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public record GetCurrenciesQuery(PaginationModel Pagination) : IQuery<GetCurrenciesResult>;

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