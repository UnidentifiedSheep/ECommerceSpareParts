using System.Globalization;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cache;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Cache.Extensions;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Currency.Application.Cache;

public class CurrencyCacheRepository(
	ICache rawCache,
	IReadRepository<Entities.Currency.Currency, int> repository,
	IProjectionProvider<Entities.Currency.Currency, CurrencyDto> projection) : ICurrencyCacheRepository
{
	public async Task<CurrencyDto?> GetCurrency(int id, CancellationToken cancellationToken = default)
	{
		var key = CurrencyCacheKeys.Currency(id);
		return await rawCache.GetOrSetAsync(
			key,
			() => GetCurrencyFromDb(id),
			CurrencyCacheKeys.Ttl);
	}

	public async Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(
		CancellationToken cancellationToken = default)
	{
		var currenciesKey = CurrencyCacheKeys.AllCurrencies();
		var allCurrencies = await rawCache.GetFromSetAsync(currenciesKey);

		if (allCurrencies.Length != 0)
			return (await rawCache.GetOrSetManyAsync(
					allCurrencies.Select(int.Parse),
					CurrencyCacheKeys.Currency,
					currency => currency.Id,
					GetMissingCurrenciesFromDb,
					CurrencyCacheKeys.Ttl))
				.Select(x => x.Value)
				.ToList();

		var currencies = await repository.Query.Project(projection).ToListAsync(cancellationToken);

		await rawCache.AddToSetAsync(
			currenciesKey,
			currencies.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)));
		return currencies;
	}

	public Task InvalidateCurrency(int id, CancellationToken cancellationToken = default)
		=> rawCache.RemoveKeyAsync(CurrencyCacheKeys.Currency(id));

	public async Task InvalidateAllCurrencies(CancellationToken cancellationToken = default)
	{
		var currenciesKey = CurrencyCacheKeys.AllCurrencies();
		var currencyIds = await rawCache.GetFromSetAsync(currenciesKey);

		var keys = currencyIds
			.Select(int.Parse)
			.Select(CurrencyCacheKeys.Currency)
			.Append(currenciesKey);

		await rawCache.RemoveKeysAsync(keys);
	}

	private Task<CurrencyDto?> GetCurrencyFromDb(int id)
		=> repository.Query.Where(x => x.Id == id).Project(projection).FirstOrDefaultAsync();

	private Task<Dictionary<int, CurrencyDto>> GetMissingCurrenciesFromDb(IEnumerable<int> ids)
		=> repository
			.Query
			.Where(x => ids.Contains(x.Id))
			.Project(projection)
			.ToDictionaryAsync(x => x.Id);
}
