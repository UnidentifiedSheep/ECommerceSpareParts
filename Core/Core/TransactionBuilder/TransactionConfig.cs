
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Core.TransactionBuilder;

public class TransactionConfig(DbContext context)
{
    private static readonly Dictionary<string, TransactionDefaultSettings> DefaultSettings = new();
    private readonly HashSet<string> _retryOn = [];
    public int RetriesCount { get; private set; }
    public TimeSpan RetryDelay { get; private set; } = TimeSpan.Zero;
    public IsolationLevel IsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;
    
    
    public TransactionConfig WithRetries(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative");
        RetriesCount = count;
        return this;
    }

    public TransactionConfig WithRetryDelay(TimeSpan delay)
    {
        RetryDelay = delay;
        return this;
    }

    public TransactionConfig WithIsolationLevel(IsolationLevel isolationLevel)
    {
        if (isolationLevel == IsolationLevel.Unspecified)
            throw new ArgumentException("Уровень изоляции не должен быть 'Unspecified'");
        IsolationLevel = isolationLevel;
        return this;
    }

    public TransactionConfig WithDefaultSettings(string variant)
    {
        var defaultSettings = DefaultSettings[variant];
        WithRetryDelay(defaultSettings.RetryDelay)
            .WithIsolationLevel(defaultSettings.IsolationLevel)
            .WithRetries(defaultSettings.RetriesCount)
            .AddRetryPgErrorKey(defaultSettings.RetryOn);
        return this;
    }

    public static void AddDefaultSettings(string variant, TransactionDefaultSettings defaultSettings)
    {
        DefaultSettings.Add(variant, defaultSettings);
    }

    public TransactionConfig AddRetryPgErrorKey(IEnumerable<string> keys)
    {
        _retryOn.UnionWith(keys);
        return this;
    }

    public async Task<T> ExecuteWithTransaction<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt <= RetriesCount; attempt++)
        {
            var isLocalTransaction = context.Database.CurrentTransaction == null;
            var transaction = context.Database.CurrentTransaction ?? await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if(isLocalTransaction)
                    await SetIsolationLevelOfDbTransaction();
                if(attempt > 0)
                    context.ChangeTracker.Clear();
                T result = await action();

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
        for (int attempt = 0; attempt <= RetriesCount; attempt++)
        {
            var isLocalTransaction = context.Database.CurrentTransaction == null;
            var transaction = context.Database.CurrentTransaction ?? await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if(isLocalTransaction)
                    await SetIsolationLevelOfDbTransaction();
                if(attempt > 0)
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

    private PostgresException? GetException(Exception? ex)
    {
        int deepness = 0;
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
        await context.Database.ExecuteSqlRawAsync($"SET TRANSACTION ISOLATION LEVEL {withSpaces};");
    }
    
}