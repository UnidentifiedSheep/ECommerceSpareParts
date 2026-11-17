using Application.Common.Interfaces.Settings;
using Main.Entities.Currency.Settings;
using Main.Modules.Currency.Abstractions.Interfaces;
using Main.Modules.Currency.Application.Interfaces;

namespace Main.Modules.Currency.Application.Services;

public sealed class CurrencyLookup(
	ICurrencyCacheRepository currencies,
	ISettingsService settingsService) : ICurrencyLookup
{
	public async Task<int?> GetCurrencyIdAsync(string code, CancellationToken cancellationToken = default)
		=> (await currencies.GetAllCurrencies(cancellationToken))
			.FirstOrDefault(x => string.Equals(
				x.Code,
				code,
				StringComparison.OrdinalIgnoreCase))
			?.Id;

	public async Task<int> GetBaseCurrencyIdAsync(CancellationToken cancellationToken = default)
		=> (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken)).Data.BaseCurrencyId;

	public async Task<IReadOnlyList<int>> GetCurrencyIdsAsync(CancellationToken cancellationToken = default)
		=> (await currencies.GetAllCurrencies(cancellationToken)).Select(x => x.Id).ToArray();
}
