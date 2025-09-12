using Core.Attributes;
using Core.Interfaces.Services;
using Persistence.Contexts;
using Persistence.TransactionBuilder;

namespace Persistence.Services;

public class UnitOfWork(DContext context) : IUnitOfWork
{
    public async Task<T> ExecuteWithTransaction<T>(TransactionalAttribute settings, Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        return await context.WithIsolationLevel(settings.IsolationLevel)
            .WithRetries(settings.RetryCount)
            .AddRetryPgErrorKey(settings.RetryErrors)
            .WithRetryDelay(TimeSpan.FromMilliseconds(settings.RetryDelayMs))
            .ExecuteWithTransaction(action, cancellationToken);
    }

    public async Task ExecuteWithTransaction(TransactionalAttribute settings, Func<Task> action, CancellationToken cancellationToken = default)
    {
        await context.WithIsolationLevel(settings.IsolationLevel)
            .AddRetryPgErrorKey(settings.RetryErrors)
            .WithRetries(settings.RetryCount)
            .WithRetryDelay(TimeSpan.FromMilliseconds(settings.RetryDelayMs))
            .ExecuteWithTransaction(action, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(entity ?? throw new ArgumentNullException(nameof(entity)), cancellationToken);
    }

    public async Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class
    {
        await context.Set<T>().AddRangeAsync(entities, cancellationToken);
    }

    public void Remove<T>(T entity)
    {
        context.Remove(entity ?? throw new ArgumentNullException(nameof(entity)));
    }

    public void RemoveRange<T>(IEnumerable<T> entities)
    {
        context.RemoveRange(entities);
    }
}