using Application.Common.Interfaces.Cqrs;
using Main.Modules.Currency.Abstractions.Interfaces;

namespace Main.Modules.Currency.Application.Handlers.Currencies.GetCurrencyRate;

public record GetCurrencyRateQuery(int CurrencyId) : IQuery<GetCurrencyRateResult>;

public record GetCurrencyRateResult(decimal Rate);

public class GetCurrencyRateHandler(ICurrencyRatesProvider currencyRatesProvider)
	: IQueryHandler<GetCurrencyRateQuery, GetCurrencyRateResult>
{
	public async Task<GetCurrencyRateResult> Handle(
		GetCurrencyRateQuery request,
		CancellationToken cancellationToken)
	{
		var rate = await currencyRatesProvider.GetRate(request.CurrencyId, cancellationToken);
		return new GetCurrencyRateResult(rate);
	}
}
