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

public sealed class CurrencyRateSnapshotProvider(
	IApplicationTransactionFactory transactionFactory
	) : ICurrencyRateSnapshotProvider, IDisposable
{
	private static readonly IComparer<Info> RateComparer =
		Comparer<Info>.Create((a, b) => a.When.CompareTo(b.When));

	private readonly SemaphoreSlim _lock = new(1, 1);
	private Snapshot? _snapshot;

	public async Task<decimal?> GetCurrentRateOrDefault(int currencyId, CancellationToken token)
	{
		var snapshot = await GetSnapshotAsync(token);
		if (currencyId == snapshot.BaseCurrencyId) return 1m;
		return snapshot.History.TryGetValue((currencyId, snapshot.BaseCurrencyId), out var rates) && !rates.IsDefaultOrEmpty
			? rates[^1].Rate : null;
	}

	public async Task<decimal> GetNearestRateFor(
		int currencyId,
		DateTime date,
		CancellationToken token)
	{
		if (date.Kind != DateTimeKind.Utc)
			throw new ArgumentException("Currency rate date must be UTC.", nameof(date));

		var snapshot = await GetSnapshotAsync(token);
		if (currencyId == snapshot.BaseCurrencyId) return 1m;
		if (!snapshot.History.TryGetValue((currencyId, snapshot.BaseCurrencyId), out var rates) || rates.IsDefaultOrEmpty)
			throw new CurrencyRateNotFoundException(currencyId);

		var rate = GetNearestRate(rates, date);
		return rate > 0m ? rate : throw new CurrencyRateNotFoundException(currencyId);
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

	public async Task Reload(CancellationToken token)
	{
		await _lock.WaitAsync(token);
		try
		{
			// Always reload after acquiring the lock: an earlier load may predate this invalidation.
			Volatile.Write(ref _snapshot, await LoadRates(token));
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<Snapshot> GetSnapshotAsync(CancellationToken token)
	{
		var snapshot = Volatile.Read(ref _snapshot);
		if (snapshot is not null) return snapshot;

		await _lock.WaitAsync(token);
		try
		{
			snapshot = Volatile.Read(ref _snapshot);
			if (snapshot is not null) return snapshot;
			snapshot = await LoadRates(token);
			Volatile.Write(ref _snapshot, snapshot);
			return snapshot;
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<Snapshot> LoadRates(CancellationToken token)
	{
		await using var transaction = transactionFactory.Create();
		int baseCurrencyId = -1;

		var history = (await transaction
			.WithSettings(new TransactionalAttribute(IsolationLevel.RepeatableRead))
			.ExecuteAsync(async (ctx, ct) =>
			{
				baseCurrencyId = (await ctx
						.GetRequiredService<ISettingsService>()
						.GetOrDefault<CurrencySetting>(ct)).Data.BaseCurrencyId;

				return await ctx
					.Repositories
					.GetForRead<CurrencyRate, int>()
					.Query
					.Select(x => new
					{
						x.Id,
						x.FromCurrencyId,
						x.ToCurrencyId,
						x.Rate,
						x.CreatedAt
					})
					.ToArrayAsync(ct);
			}, token))
			.GroupBy(
				x => (x.FromCurrencyId, x.ToCurrencyId),
				x => x)
			.ToFrozenDictionary(
				x => x.Key,
				x => x.OrderByDescending(row => row.Id)
					.DistinctBy(row => row.CreatedAt)
					.OrderBy(row => row.CreatedAt)
					.Select(row => new Info(row.Rate, row.CreatedAt)).ToImmutableArray());

		return new Snapshot(baseCurrencyId, history);
	}

	private sealed record Snapshot(
		int BaseCurrencyId,
		FrozenDictionary<(int FromCurrencyId, int ToCurrencyId), ImmutableArray<Info>> History);

	private readonly record struct Info(decimal Rate, DateTime When);

	public void Dispose() => _lock.Dispose();
}
