namespace Main.Modules.Currency.Abstractions;

public interface ICurrencyLookup
{
	Task<int?> GetCurrencyIdAsync(string code, CancellationToken cancellationToken = default);

	Task<int> GetBaseCurrencyIdAsync(CancellationToken cancellationToken = default);

	Task<IReadOnlyList<int>> GetCurrencyIdsAsync(CancellationToken cancellationToken = default);
}
