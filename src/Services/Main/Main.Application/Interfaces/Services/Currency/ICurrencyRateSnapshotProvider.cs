namespace Main.Application.Interfaces.Services.Currency;

public interface ICurrencyRateSnapshotProvider
{
	Task<decimal?> GetCurrentRateOrDefault(int currencyId, CancellationToken token);

	Task<decimal> GetNearestRateFor(
		int currencyId,
		DateTime date,
		CancellationToken token);

	Task Reload(CancellationToken token);
}
