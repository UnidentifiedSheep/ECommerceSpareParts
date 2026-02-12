using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Currencies;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.Currencies.GetAllCurrencies;

public record GetAllCurrenciesQuery() : IQuery<GetAllCurrenciesResult>;
public record GetAllCurrenciesResult(List<CurrencyWithRateDto> Currencies);

public class GetAllCurrenciesHandler(ICurrencyRepository currencyRepository) 
    : IQueryHandler<GetAllCurrenciesQuery, GetAllCurrenciesResult>
{
    public async Task<GetAllCurrenciesResult> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await currencyRepository.GetCurrencies(null, null, false, cancellationToken,
            x => x.CurrencyToUsd);
        
        return new GetAllCurrenciesResult(currencies.Adapt<List<CurrencyWithRateDto>>());
    }
}