using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.Handlers.Currencies.GetCurrencyRates;

public record GetCurrencyRatesQuery : IQuery<GetCurrencyRatesResult>;

public record GetCurrencyRatesResult(Dictionary<int, decimal> Rates);

public class GetCurrencyRatesHandler(ICurrencyRepository currencyRepository)
    : IQueryHandler<GetCurrencyRatesQuery, GetCurrencyRatesResult>
{
    public async Task<GetCurrencyRatesResult> Handle(GetCurrencyRatesQuery request, CancellationToken cancellationToken)
    {
        var toUsd = await currencyRepository.GetCurrenciesToUsd(cancellationToken);
        return new GetCurrencyRatesResult(toUsd);
    }
}