using Application.Common.Interfaces.Repositories;
using Main.Modules.Currency.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Currency.Application.Services;

public sealed class CurrencyDisplay(IReadRepository<Entities.Currency.Currency, int> currencies)
	: ICurrencyDisplay
{
	public async Task<CurrencyDisplayInfo?> GetAsync(
		int currencyId,
		CancellationToken cancellationToken = default)
		=> await currencies.Query
			.Where(currency => currency.Id == currencyId)
			.Select(currency => new CurrencyDisplayInfo(currency.Code, currency.CurrencySign))
			.FirstOrDefaultAsync(cancellationToken);
}
