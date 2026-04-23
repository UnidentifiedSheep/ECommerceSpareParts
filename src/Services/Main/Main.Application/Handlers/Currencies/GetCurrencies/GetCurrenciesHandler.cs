using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Projections;
using Main.Entities.Currency;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Currencies.GetCurrencies;

public record GetCurrenciesQuery(PaginationModel Pagination) : IQuery<GetCurrenciesResult>;

public record GetCurrenciesResult(IEnumerable<CurrencyDto> Currencies);

public class GetCurrenciesHandler(
    IReadRepository<Currency, int> repository, 
    IRelatedDataCollector relatedDataCollector)
    : IQueryHandler<GetCurrenciesQuery, GetCurrenciesResult>
{
    public async Task<GetCurrenciesResult> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .AsExpandable()
            .Select(CurrencyProjections.ToDto)
            .ApplyPagination(request.Pagination)
            .ToListAsync(cancellationToken);

        relatedDataCollector.AddRange(result.Select(x => x.Id.ToString()));

        return new GetCurrenciesResult(result);
    }
}