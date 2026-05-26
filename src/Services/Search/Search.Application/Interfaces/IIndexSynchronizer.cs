namespace Search.Application.Interfaces;

public interface IIndexSynchronizer<TEntity, in TKey>
{
    Task Reindex(TKey id, CancellationToken cancellationToken = default);

    Task Delete(TKey id, CancellationToken cancellationToken = default);
}