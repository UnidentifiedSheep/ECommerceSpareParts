using Domain;

namespace Analytics.Application.Interfaces.Services.FactSynchronizers;

public interface IFactSynchronizer<TEntity, TKey>
    where TEntity : Entity<TEntity, TKey>
    where TKey : notnull
{
    Task<TEntity?> SynchronizeAsync(
        TKey id,
        CancellationToken cancellationToken = default);
}