using Application.Common.Interfaces.Currency;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Application.Services;

public class CurrencyRatesProvider(
    ICachedCurrencyProvider cachedCurrencyProvider
) : ICurrencyRatesProvider
{
    public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return await cachedCurrencyProvider.GetCurrencyRate(currencyId, cancellationToken) ??
               throw new InvalidOperationException("Currency rate not found");
    }

    public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default)
    {
        return cachedCurrencyProvider.GetCurrencyRate(currencyId, cancellationToken);
    }
}