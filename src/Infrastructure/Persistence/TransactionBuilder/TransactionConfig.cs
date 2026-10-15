using System.Data;
using Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.TransactionBuilder;

public class TransactionConfig(DbContext context) : ICustomTransaction
{
	private static readonly Dictionary<string, TransactionDefaultSettings> DefaultSettings = new();

	private readonly HashSet<string> _retryOn = [];

	public int RetriesCount { get; private set; }

	public TimeSpan RetryDelay { get; private set; } = TimeSpan.Zero;

	public IsolationLevel IsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;

	public ICustomTransaction WithRetries(int count)
	{
		if (count < 0)
			throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative");
		RetriesCount = count;
		return this;
	}

	public ICustomTransaction WithRetryDelay(TimeSpan delay)
	{
		RetryDelay = delay;
		return this;
	}

	public ICustomTransaction WithIsolationLevel(IsolationLevel isolationLevel)
	{
		if (isolationLevel == IsolationLevel.Unspecified)
			throw new ArgumentException("Уровень изоляции не должен быть 'Unspecified'");
		IsolationLevel = isolationLevel;
		return this;
	}

	public ICustomTransaction WithDefaultSettings(string variant)
	{
		var defaultSettings = DefaultSettings[variant];
		WithRetryDelay(defaultSettings.RetryDelay)
			.WithIsolationLevel(defaultSettings.IsolationLevel)
			.WithRetries(defaultSettings.RetriesCount)
			.AddRetryPgErrorKey(defaultSettings.RetryOn);
		return this;
	}

	public ICustomTransaction AddRetryPgErrorKey(IEnumerable<string> keys)
	{
		_retryOn.UnionWith(keys);
		return this;
	}

	public async Task<T> ExecuteWithTransaction<T>(
		Func<Task<T>> action,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);
		cancellationToken.ThrowIfCancellationRequested();
		if (context.Database.CurrentTransaction is not null)
			return await action();

		for (var attempt = 0; ; attempt++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await using (var transaction = await context.Database.BeginTransactionAsync(IsolationLevel, cancellationToken))
			{
				try
				{
					if (attempt > 0)
						context.ChangeTracker.Clear();
					var result = await action();
					await transaction.CommitAsync(cancellationToken);
					return result;
				}
				catch (Exception exception)
				{
					await transaction.RollbackAsync(CancellationToken.None);
					if (exception is OperationCanceledException || cancellationToken.IsCancellationRequested ||
					    attempt >= RetriesCount || !IsRetryable(exception))
						throw;
				}
			}

			await Task.Delay(RetryDelay, cancellationToken);
		}
	}

	public Task ExecuteWithTransaction(Func<Task> action, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(action);
		return ExecuteWithTransaction(async () =>
		{
			await action();
			return true;
		}, cancellationToken);
	}

	private bool IsRetryable(Exception exception) =>
		exception is DbUpdateConcurrencyException ||
		PostgresTransactionExceptionClassifier.FindPostgresException(exception) is { } postgres && _retryOn.Contains(postgres.SqlState);

	public static void AddDefaultSettings(string variant, TransactionDefaultSettings defaultSettings) =>
		DefaultSettings.Add(variant, defaultSettings);
}
