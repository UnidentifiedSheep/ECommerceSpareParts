using Application.Common.Extensions;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Currency.Application.Dtos;
using Main.Modules.Currency.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace Main.Modules.Currency.Application.Cache;

public class CurrencyCacheRepository(
	IFusionCache cache,
	IReadRepository<Entities.Currency.Currency, int> repository,
	IProjectionProvider<Entities.Currency.Currency, CurrencyDto> projection) : ICurrencyCacheRepository
{
	public async Task<CurrencyDto?> GetCurrency(int id, CancellationToken cancellationToken = default)
		=> (await GetCurrenciesAsync(cancellationToken)).GetValueOrDefault(id);

	public async Task<IReadOnlyList<CurrencyDto>> GetAllCurrencies(CancellationToken cancellationToken = default)
		=> (await GetCurrenciesAsync(cancellationToken)).Values.OrderBy(currency => currency.Id).ToArray();

	public async Task InvalidateAllCurrencies(CancellationToken cancellationToken = default)
		=> await cache.RemoveAsync(CurrencyCacheKeys.Currencies, token: cancellationToken);

	private async Task<Dictionary<int, CurrencyDto>> GetCurrenciesAsync(CancellationToken cancellationToken)
		=> await cache.GetOrSetAsync(
			CurrencyCacheKeys.Currencies,
			GetCurrenciesFromDbAsync,
			new FusionCacheEntryOptions(CurrencyCacheKeys.Ttl),
			cancellationToken);

	private async Task<Dictionary<int, CurrencyDto>> GetCurrenciesFromDbAsync(CancellationToken cancellationToken)
		=> await repository.Query
			.Project(projection)
			.ToDictionaryAsync(currency => currency.Id, cancellationToken);
}
