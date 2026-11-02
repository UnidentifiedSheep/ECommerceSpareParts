using Application.Common.Interfaces.Currency;
using Main.Application.Interfaces.Services.Currency;
using Main.Entities.Exceptions;

namespace Main.Application.Services.Currency;

public class CurrencyRatesProvider(
	ICurrencyRateSnapshotProvider history) : ICurrencyRatesProvider
{
	public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default) =>
		await history.GetCurrentRateOrDefault(currencyId, cancellationToken) ?? throw new CurrencyRateNotFoundException(currencyId);

	public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default) =>
		history.GetCurrentRateOrDefault(currencyId, cancellationToken);

	public Task<decimal> GetRate(int currencyId, DateTime forDate, CancellationToken cancellationToken = default)
		=> history.GetNearestRateFor(currencyId, forDate, cancellationToken);
}
