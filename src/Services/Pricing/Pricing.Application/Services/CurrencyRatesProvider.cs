using Internal.Integration.Core.Interfaces.Main;
using Application.Common.Interfaces.Currency;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Application.Services;

public class CurrencyRatesProvider(ICachedCurrencyProvider cachedCurrencyProvider, IMainClient mainClient) : ICurrencyRatesProvider
{
	public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
	{
		return await cachedCurrencyProvider.GetCurrencyRate(currencyId, cancellationToken) ??
			throw new InvalidOperationException("Currency rate not found");
	}

	public async Task<decimal> GetRate(int currencyId, DateTime forDate, CancellationToken cancellationToken = default)
	{
		if (forDate.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Currency rate date must be UTC.", nameof(forDate));
		var response = await mainClient.CurrencyNode.GetCurrencyRate(currencyId, forDate, cancellationToken);
		if (!response.Success)
			throw new InvalidOperationException($"Unable to get historical currency rate for {currencyId}: HTTP {(int)response.StatusCode}.");
		return response.ValueOrThrow;
	}

	public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default) =>
		cachedCurrencyProvider.GetCurrencyRate(currencyId, cancellationToken);
}
