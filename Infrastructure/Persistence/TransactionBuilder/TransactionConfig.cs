using System.Data;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task<T> ExecuteWithTransaction<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt <= RetriesCount; attempt++)
        {
            var isLocalTransaction = context.Database.CurrentTransaction == null;
            var transaction = context.Database.CurrentTransaction ??
                              await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (isLocalTransaction)
                    await SetIsolationLevelOfDbTransaction();
                if (attempt > 0)
                    context.ChangeTracker.Clear();
                var result = await action();

                if (isLocalTransaction)
                    await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                if (!isLocalTransaction) throw;
                await transaction.RollbackAsync(cancellationToken);

                var pgEx = GetException(ex);
                if (pgEx == null || !_retryOn.Contains(pgEx.SqlState)) throw;
                await Task.Delay(RetryDelay, cancellationToken);
            }
            finally
            {
                if (isLocalTransaction)
                    await transaction.DisposeAsync();
            }
        }

        throw new InvalidOperationException("Out of attempts");
    }

    public async Task ExecuteWithTransaction(Func<Task> action, CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt <= RetriesCount; attempt++)
        {
            var isLocalTransaction = context.Database.CurrentTransaction == null;
            var transaction = context.Database.CurrentTransaction ??
                              await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (isLocalTransaction)
                    await SetIsolationLevelOfDbTransaction();
                if (attempt > 0)
                    context.ChangeTracker.Clear();
                await action();

                if (isLocalTransaction)
                    await transaction.CommitAsync(cancellationToken);
                return;
            }
            catch (Exception ex)
            {
                if (!isLocalTransaction) throw;
                await transaction.RollbackAsync(cancellationToken);

                var pgEx = GetException(ex);
                if (pgEx == null || !_retryOn.Contains(pgEx.SqlState)) throw;
                await Task.Delay(RetryDelay, cancellationToken);
            }
            finally
            {
                if (isLocalTransaction)
                    await transaction.DisposeAsync();
            }
        }

        throw new InvalidOperationException("Out of attempts");
    }

    public static void AddDefaultSettings(string variant, TransactionDefaultSettings defaultSettings)
    {
        DefaultSettings.Add(variant, defaultSettings);
    }

    private PostgresException? GetException(Exception? ex)
    {
        var deepness = 0;
        var tempEx = ex;

        while (tempEx != null && !(tempEx is PostgresException) && deepness < 4)
        {
            tempEx = tempEx.InnerException;
            deepness++;
        }

        return tempEx as PostgresException;
    }

    private async Task SetIsolationLevelOfDbTransaction()
    {
        var isolationLevelAsString = Enum.GetName(typeof(IsolationLevel), IsolationLevel)!;
        var withSpaces = string.Concat(isolationLevelAsString.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? " " + c : c.ToString())).ToUpperInvariant();
        var sql = $"SET TRANSACTION ISOLATION LEVEL {withSpaces};";
        await context.Database.ExecuteSqlRawAsync(sql);
    }
}