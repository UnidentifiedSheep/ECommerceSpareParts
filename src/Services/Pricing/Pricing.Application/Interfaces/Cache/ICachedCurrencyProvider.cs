namespace Pricing.Application.Interfaces.Cache;

public interface ICachedCurrencyProvider
{
    Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task<int?> GetCurrencyIdAsync(string code, CancellationToken token = default);
    ValueTask<int> GetBaseCurrencyIdAsync(CancellationToken token = default);
}