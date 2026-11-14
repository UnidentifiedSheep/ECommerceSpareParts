using Application.Common.Interfaces.Settings;
using Main.Application.Interfaces.Cache;
using Main.Entities.Settings;
using Pricing.Application.Interfaces.Cache;

namespace Pricing.Application.Services;

public sealed class CurrencyLookup(
	ICurrencyCacheRepository currencies,
	ISettingsService settingsService) : ICurrencyLookup
{
	public async Task<int?> GetCurrencyIdAsync(string code, CancellationToken token = default)
		=> (await currencies.GetAllCurrencies(token))
			.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))
			?.Id;

	public async Task<int> GetBaseCurrencyIdAsync(CancellationToken token = default)
		=> (await settingsService.GetOrDefault<CurrencySetting>(token)).Data.BaseCurrencyId;

	public async Task<IReadOnlyList<int>> GetCurrencyIdsAsync(CancellationToken token = default)
		=> (await currencies.GetAllCurrencies(token)).Select(x => x.Id).ToArray();
}
