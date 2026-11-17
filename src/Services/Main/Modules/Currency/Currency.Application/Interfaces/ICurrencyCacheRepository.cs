using Main.Modules.Currency.Application.Dtos;

namespace Main.Modules.Currency.Application.Interfaces;

public interface ICurrencyCacheRepository
{
	Task<CurrencyDto?> GetCurrency(int id, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(CancellationToken cancellationToken = default);

	Task InvalidateCurrency(int id, CancellationToken cancellationToken = default);

	Task InvalidateAllCurrencies(CancellationToken cancellationToken = default);
}
