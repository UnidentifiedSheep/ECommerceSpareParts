namespace Search.Application.Interfaces;

public interface IIndexSynchronizer<TEntity, in TKey>
{
    Task Reindex(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

    Task Delete(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
}