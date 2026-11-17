using Main.Modules.Currency.Abstractions.Events;

namespace Main.Modules.Currency.Abstractions.Interfaces;

public interface ICurrencyRateSnapshotProvider
{
	Task<decimal?> GetCurrentRateOrDefault(int currencyId, CancellationToken token);

	Task<decimal> GetNearestRateFor(
		int currencyId,
		DateTime date,
		CancellationToken token);

	Task Reload(CancellationToken token);

	Task Apply(IReadOnlyCollection<CurrencyRateChange> changes, CancellationToken token);
}
