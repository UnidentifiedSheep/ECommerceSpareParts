namespace Domain.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey>
{
    ValueTask<TEntity?> GetById(
        TKey id, 
        ISpecification<TEntity, TKey>? spec = null, 
        CancellationToken ct = default);
}