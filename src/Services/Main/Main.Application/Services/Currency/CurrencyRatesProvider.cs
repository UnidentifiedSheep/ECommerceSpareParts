using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Main.Application.Interfaces.Cache;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.Currency;

public class CurrencyRatesProvider(ICurrencyCacheRepository cacheRepository,
	IReadRepository<CurrencyRateHistory, int> history, ISettingsService settings) : ICurrencyRatesProvider
{
	private readonly Dictionary<(int CurrencyId, int BaseCurrencyId, DateTime Date), decimal> _historicalRates = [];

	public async Task<decimal> GetRate(int currencyId, CancellationToken cancellationToken = default) =>
		await cacheRepository.GetCurrencyRate(currencyId, cancellationToken) ?? throw new CurrencyRateNotFoundException(currencyId);

	public Task<decimal?> GetRateOrDefault(int currencyId, CancellationToken cancellationToken = default) =>
		cacheRepository.GetCurrencyRate(currencyId, cancellationToken);

	public async Task<decimal> GetRate(int currencyId, DateTime forDate, CancellationToken cancellationToken = default)
	{
		if (forDate.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Currency rate date must be UTC.", nameof(forDate));
		var baseCurrencyId = (await settings.GetOrDefault<CurrencySetting>(cancellationToken)).Data.BaseCurrencyId;
		if (currencyId == baseCurrencyId) return 1m;
		var key = (currencyId, baseCurrencyId, forDate);
		if (_historicalRates.TryGetValue(key, out var cached)) return cached;
		var query = history.Query.Where(item => item.FromCurrencyId == currencyId && item.ToCurrencyId == baseCurrencyId);
		var rate = await query.Where(item => item.CreatedAt <= forDate)
			.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id)
			.Select(item => (decimal?)item.NewRate).FirstOrDefaultAsync(cancellationToken);
		if (rate is null)
			rate = await query.Where(item => item.CreatedAt > forDate).OrderBy(item => item.CreatedAt).ThenBy(item => item.Id)
				.Select(item => (decimal?)item.PrevRate).FirstOrDefaultAsync(cancellationToken);
		if (rate is null or <= 0) throw new CurrencyRateNotFoundException(currencyId);
		_historicalRates.Add(key, rate.Value);
		return rate.Value;
	}
}
