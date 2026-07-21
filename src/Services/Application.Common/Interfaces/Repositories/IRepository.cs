using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
    ValueTask<TEntity?> GetById(
        TKey id,
        CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default);

    IAsyncEnumerable<TEntity> AsyncEnumerable(
        Criteria<TEntity>? criteria = null);

    Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default);
    
    Task DeleteManyAsync(
        IEnumerable<TKey> ids, 
        CancellationToken cancellationToken = default);
}