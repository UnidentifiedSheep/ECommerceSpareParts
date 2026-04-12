using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey>
{
    ValueTask<TEntity?> GetById(
        TKey id, 
        Criteria<TEntity>? criteria = null, 
        CancellationToken ct = default);
    
    Task<TEntity?> FirstOrDefaultAsync(
        Criteria<TEntity>? criteria = null, 
        CancellationToken ct = default);

    Task<List<TEntity>> ListAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default);
}