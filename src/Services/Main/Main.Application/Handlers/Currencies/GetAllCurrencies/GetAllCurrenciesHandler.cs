using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Projections;
using Main.Entities.Currency;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Currencies.GetAllCurrencies;

public record GetAllCurrenciesQuery : IQuery<GetAllCurrenciesResult>;

public record GetAllCurrenciesResult(List<CurrencyDto> Currencies);

public class GetAllCurrenciesHandler(IReadRepository<Currency, int> repository)
    : IQueryHandler<GetAllCurrenciesQuery, GetAllCurrenciesResult>
{
    public async Task<GetAllCurrenciesResult> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .AsExpandable()
            .Select(CurrencyProjections.ToDto)
            .ToListAsync(cancellationToken);

        return new GetAllCurrenciesResult(result);
    }
}