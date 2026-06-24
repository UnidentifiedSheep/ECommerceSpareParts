using Application.Common.Interfaces.Currency;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;

namespace Main.Application.Services.Currency;

public class CurrencyRatesProvider(
    ICurrencyCacheRepository cacheRepository) : ICurrencyRatesProvider
{
    public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default)
    {
        return await cacheRepository.GetCurrencyRate(currencyId, cancellationToken) ?? 
               throw new CurrencyRateNotFoundException(currencyId);
    }

    public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default)
    {
        return cacheRepository.GetCurrencyRate(currencyId, cancellationToken);
    }
}