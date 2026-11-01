using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Data;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Interfaces.Services.Currency;
using Main.Entities.Currency;
using Main.Entities.Exceptions;
using Main.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Services.Currency;

public sealed class HistoricalRateProvider(
	IApplicationTransactionFactory transactionFactory
	) : IHistoricalRateProvider, IDisposable
{
	private static readonly IComparer<Info> RateComparer =
		Comparer<Info>.Create((a, b) => a.When.CompareTo(b.When));

	private readonly SemaphoreSlim _lock = new(1, 1);
	private bool _loaded;

	private FrozenDictionary<(int FromCurrencyId, int ToCurrencyId), ImmutableArray<Info>> _history
		= new Dictionary<(int FromCurrencyId, int ToCurrencyId),ImmutableArray<Info>>().ToFrozenDictionary();

	private int _baseCurrencyId = -1;

	public async Task<decimal> GetNearestRateFor(
		int currencyId,
		DateTime date,
		CancellationToken token)
	{
		await EnsureLoadedAsync(token);
		if (!_history.TryGetValue((currencyId, _baseCurrencyId), out var arr) ||
			arr.IsDefaultOrEmpty)
			throw new CurrencyRateNotFoundException(currencyId);

		return GetNearestRate(arr, date);
	}

	private static decimal GetNearestRate(
		ImmutableArray<Info> rates,
		DateTime date)
	{
		var index = rates.BinarySearch(new Info(0, date), RateComparer);

		if (index >= 0)
			return rates[index].Rate;

		index = ~index;

		if (index == 0)
			return rates[0].Rate;

		if (index == rates.Length)
			return rates[^1].Rate;

		var before = rates[index - 1];
		var after = rates[index];

		return date.Ticks - before.When.Ticks <= after.When.Ticks - date.Ticks
			? before.Rate
			: after.Rate;
	}

	public Task Reload(CancellationToken token)
	{
		Volatile.Write(ref _loaded, false);
		return EnsureLoadedAsync(token);
	}

	private async Task EnsureLoadedAsync(CancellationToken ct)
	{
		if (Volatile.Read(ref _loaded)) return;
		await _lock.WaitAsync(ct);

		try
		{
			if (_loaded) return;

			await LoadRates(ct);
			Volatile.Write(ref _loaded, true);
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task LoadRates(CancellationToken token)
	{
		await using var transaction = transactionFactory.Create();
		int baseCurrencyId = -1;

		_history = (await transaction
			.WithSettings(new TransactionalAttribute(IsolationLevel.RepeatableRead))
			.ExecuteAsync(async (ctx, ct) =>
			{
				baseCurrencyId = (await ctx
						.GetRequiredService<ISettingsService>()
						.GetOrDefault<CurrencySetting>(ct)).Data.BaseCurrencyId;

				return await ctx
					.Repositories
					.GetForRead<CurrencyRateHistory, int>()
					.Query
					.Select(x => new
					{
						x.FromCurrencyId,
						x.ToCurrencyId,
						x.NewRate,
						x.CreatedAt
					})
					.ToArrayAsync(ct);
			}, token))
			.GroupBy(
				x => (x.FromCurrencyId, x.ToCurrencyId),
				x => new Info(x.NewRate, x.CreatedAt))
			.ToFrozenDictionary(
				x => x.Key,
				x => x.OrderBy(z => z.When).ToImmutableArray());

		_baseCurrencyId = baseCurrencyId;
	}

	private readonly record struct Info(decimal Rate, DateTime When);

	public void Dispose() => _lock.Dispose();
}
