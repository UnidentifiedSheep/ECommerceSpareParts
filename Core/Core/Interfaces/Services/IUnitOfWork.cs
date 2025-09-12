using Core.Attributes;

namespace Core.Interfaces.Services;

public interface IUnitOfWork
{
    Task<T> ExecuteWithTransaction<T>(TransactionalAttribute settings, Func<Task<T>> action,
        CancellationToken cancellationToken = default);

    Task ExecuteWithTransaction(TransactionalAttribute settings, Func<Task> action,
        CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class;
    void Remove<T>(T entity);
    void RemoveRange<T>(IEnumerable<T> entities);
}