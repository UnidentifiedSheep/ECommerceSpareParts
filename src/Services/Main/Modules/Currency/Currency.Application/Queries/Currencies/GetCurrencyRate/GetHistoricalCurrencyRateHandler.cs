using Application.Common.Interfaces.Cqrs;
using Main.Modules.Currency.Abstractions.Interfaces;

namespace Main.Modules.Currency.Application.Queries.Currencies.GetCurrencyRate;

public sealed record GetHistoricalCurrencyRateQuery(int CurrencyId, DateTimeOffset ForDate)
	: IQuery<GetCurrencyRateResult>;

public sealed class GetHistoricalCurrencyRateHandler(ICurrencyRatesProvider rates)
	: IQueryHandler<GetHistoricalCurrencyRateQuery, GetCurrencyRateResult>
{
	public async Task<GetCurrencyRateResult> Handle(
		GetHistoricalCurrencyRateQuery request,
		CancellationToken cancellationToken)
		=> new(
			await rates.GetRate(
				request.CurrencyId,
				request.ForDate.UtcDateTime,
				cancellationToken));
}
