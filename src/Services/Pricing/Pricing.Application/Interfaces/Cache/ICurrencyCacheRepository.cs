namespace Pricing.Application.Interfaces.Cache;

public interface ICurrencyCacheRepository
{
    Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
}