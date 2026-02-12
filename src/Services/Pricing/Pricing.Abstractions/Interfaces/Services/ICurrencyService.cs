using Pricing.Abstractions.Models;

namespace Pricing.Abstractions.Interfaces.Services;

public interface ICurrencyService
{
    Task<List<Currency>> GetCurrencies(CancellationToken cancellationToken = default);
    /// <summary>
    /// Reloads currencies from the external service. Also sets them to the cache.
    /// </summary>
    /// <returns>Loaded currencies.</returns>
    Task<List<Currency>> ReloadCurrencies(CancellationToken cancellationToken = default);
}