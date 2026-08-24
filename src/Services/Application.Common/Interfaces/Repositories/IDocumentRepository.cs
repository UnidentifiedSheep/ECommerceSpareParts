using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IDocumentRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TKey : notnull
    where TEntity : Entity<TEntity, TKey>
{
    void Add(TEntity entity);

    void Add(IEnumerable<TEntity> entities);

    void Insert(TEntity entity);

    void Insert(IEnumerable<TEntity> entities);

    void Delete(TEntity entity);

    void Delete(IEnumerable<TEntity> entities);

    void Upsert(TEntity entity);

    void Upsert(IEnumerable<TEntity> entities);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
