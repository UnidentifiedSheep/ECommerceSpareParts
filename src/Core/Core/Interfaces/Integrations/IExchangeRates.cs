using Core.Models.ExchangeRates;

namespace Core.Interfaces.Integrations;

public interface IExchangeRates
{
    Task<LatestCurrencyRatesResponse> GetRates(IEnumerable<string> currencies, string baseCurrency,
        CancellationToken cancellationToken = default);
}