using Application.Common.Interfaces;
using Main.Modules.Currency.Abstractions.Models;
using Main.Modules.Currency.Application.Interfaces;

namespace Main.Modules.Currency.Application.Services;

public sealed class CurrencyDisplay(ICurrencyCacheRepository currencies) : IDisplay<int, CurrencyDisplayInfo>
{
	public async Task<IReadOnlyDictionary<int, CurrencyDisplayInfo>> GetAsync(
		IReadOnlyCollection<int> ids,
		CancellationToken cancellationToken = default)
		=> (await currencies.GetAllCurrencies(cancellationToken))
			.Where(currency => ids.Contains(currency.Id))
			.ToDictionary(
				currency => currency.Id,
				currency => new CurrencyDisplayInfo(currency.Code, currency.CurrencySign));
}
