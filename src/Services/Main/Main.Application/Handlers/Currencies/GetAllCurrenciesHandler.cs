using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Currencies;
using Main.Application.Interfaces.Cache;

namespace Main.Application.Handlers.Currencies;

public record GetAllCurrenciesQuery : IQuery<GetAllCurrenciesResult>;

public record GetAllCurrenciesResult(IReadOnlyList<CurrencyDto> Currencies);

public class GetAllCurrenciesHandler(
    ICurrencyCacheRepository cacheRepository)
    : IQueryHandler<GetAllCurrenciesQuery, GetAllCurrenciesResult>
{
    public async Task<GetAllCurrenciesResult> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
    {
        return new GetAllCurrenciesResult(await cacheRepository.GetAllCurrencies(cancellationToken));
    }
}