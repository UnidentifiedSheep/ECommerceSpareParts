namespace Pricing.Application.Interfaces.Cache;

public interface ICurrencyLookup
{
	Task<int?> GetCurrencyIdAsync(string code, CancellationToken token = default);

	Task<int> GetBaseCurrencyIdAsync(CancellationToken token = default);

	Task<IReadOnlyList<int>> GetCurrencyIdsAsync(CancellationToken token = default);
}
