using Application.Common.Interfaces.Cqrs;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Interfaces;

namespace Main.Modules.Currency.Application.Handlers.Currencies;

public record GetAllCurrenciesQuery : IQuery<GetAllCurrenciesResult>;

public record GetAllCurrenciesResult(IReadOnlyList<CurrencyDto> Currencies);

public class GetAllCurrenciesHandler(ICurrencyCacheRepository cacheRepository)
	: IQueryHandler<GetAllCurrenciesQuery, GetAllCurrenciesResult>
{
	public async Task<GetAllCurrenciesResult> Handle(
		GetAllCurrenciesQuery request,
		CancellationToken cancellationToken)
		=> new(await cacheRepository.GetAllCurrencies(cancellationToken));
}
