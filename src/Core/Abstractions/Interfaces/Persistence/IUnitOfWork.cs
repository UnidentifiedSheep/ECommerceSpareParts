using Abstractions.Models;
using Attributes;

namespace Abstractions.Interfaces.Persistence;

public interface IUnitOfWork
{
    UnitOfWorkContext Context { get; }

    Task<T> ExecuteWithTransaction<T>(
        TransactionalAttribute settings,
        Func<Task<T>> action,
        CancellationToken cancellationToken = default);

    Task ExecuteWithTransaction(
        TransactionalAttribute settings,
        Func<Task> action,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class;
    
    Task ReloadAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;

    void Remove<T>(T entity);
    void RemoveRange<T>(IEnumerable<T> entities) where T : class;
}