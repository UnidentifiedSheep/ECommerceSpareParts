using Main.Application.Dtos.Currencies;

namespace Main.Application.Interfaces.Cache;

public interface ICurrencyCacheRepository
{
    Task<CurrencyDto?> GetCurrency(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(CancellationToken cancellationToken = default);
    Task<decimal?> GetCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
    Task InvalidateCurrency(int id, CancellationToken cancellationToken = default);
    Task InvalidateAllCurrencies(CancellationToken cancellationToken = default);
    Task InvalidateCurrencyRate(int currencyId, CancellationToken cancellationToken = default);
}