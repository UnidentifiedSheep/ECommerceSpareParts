namespace Search.Application.Interfaces;

public interface ISearchRepository<TDocument, TKey>
    where TDocument : class
    where TKey : notnull
{
    Task<TDocument?> GetById(
        TKey id,
        CancellationToken cancellationToken = default);

    Task Upsert(
        TDocument document,
        CancellationToken cancellationToken = default);

    Task UpsertMany(
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default);

    Task Delete(
        TKey id,
        CancellationToken cancellationToken = default);

    Task DeleteMany(
        IEnumerable<TKey> ids,
        CancellationToken cancellationToken = default);
}
