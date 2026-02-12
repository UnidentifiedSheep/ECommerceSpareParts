using Pricing.Abstractions.Models;

namespace Pricing.Abstractions.Interfaces.CacheRepositories;

public interface ICurrencyCacheRepository
{
    Task<List<Currency>?> GetCurrencies();
    Task SetCurrencies(List<Currency> currencies);
    Task DeleteCurrencies();
}