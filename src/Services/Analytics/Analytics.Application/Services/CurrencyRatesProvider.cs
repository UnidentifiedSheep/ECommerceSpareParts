using Analytics.Application.Interfaces.Cache;
using Application.Common.Interfaces.Currency;

namespace Analytics.Application.Services;

public class CurrencyRatesProvider(
    ICurrencyCacheRepository currencyCacheRepository
) : ICurrencyRatesProvider
{
    public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return await currencyCacheRepository.GetCurrencyRate(currencyId, cancellationToken) ??
               throw new InvalidOperationException("Currency rate not found");
    }

    public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default)
    {
        return currencyCacheRepository.GetCurrencyRate(currencyId, cancellationToken);
    }
}