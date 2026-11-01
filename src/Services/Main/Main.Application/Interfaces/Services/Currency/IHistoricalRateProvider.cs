namespace Main.Application.Interfaces.Services.Currency;

public interface IHistoricalRateProvider
{
	Task<decimal> GetNearestRateFor(
		int currencyId,
		DateTime date,
		CancellationToken token);

	Task Reload(CancellationToken token);
}
