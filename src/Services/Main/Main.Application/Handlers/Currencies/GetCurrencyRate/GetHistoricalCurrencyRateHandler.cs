using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;

namespace Main.Application.Handlers.Currencies.GetCurrencyRate;

public sealed record GetHistoricalCurrencyRateQuery(int CurrencyId, DateTimeOffset ForDate) : IQuery<GetCurrencyRateResult>;

public sealed class GetHistoricalCurrencyRateHandler(ICurrencyRatesProvider rates)
	: IQueryHandler<GetHistoricalCurrencyRateQuery, GetCurrencyRateResult>
{
	public async Task<GetCurrencyRateResult> Handle(GetHistoricalCurrencyRateQuery request, CancellationToken cancellationToken) =>
		new(await rates.GetRate(request.CurrencyId, request.ForDate.UtcDateTime, cancellationToken));
}
