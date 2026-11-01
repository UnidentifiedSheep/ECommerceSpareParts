namespace Application.Common.Interfaces.Currency;

public interface ICurrencyRatesProvider
{
	Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default);

	Task<decimal> GetRate(int currencyId, DateTime forDate, CancellationToken cancellationToken = default);

	Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default);
}
