using Application.Common.Interfaces.Repositories;
using Domain;
using Marten;

namespace Persistence.Repository.Document;

public class DocumentRepository<TEntity, TKey>(
    IDocumentSession session)
    : IDocumentRepository<TEntity, TKey>
    where TKey : notnull
    where TEntity : Entity<TEntity, TKey>
{
    public ValueTask<TEntity?> GetById(
        TKey id,
        CancellationToken ct = default)
    {
        return new ValueTask<TEntity?>(session.LoadAsync<TEntity>(id, ct));
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        return await ApplyCriteria(session.Query<TEntity>(), criteria)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        var result = await ApplyCriteria(session.Query<TEntity>(), criteria)
            .ToListAsync(ct);

        return result.ToList();
    }

    public IAsyncEnumerable<TEntity> AsyncEnumerable(
        Criteria<TEntity>? criteria = null)
    {
        return ApplyCriteria(session.Query<TEntity>(), criteria)
            .ToAsyncEnumerable();
    }

    public async Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ValidateCriteria(criteria);

        var keys = ids.Distinct().ToList();
        if (keys.Count == 0) return [];

        var documents = await LoadManyAsync(keys, ct);
        var query = ApplyCriteria(documents.AsQueryable(), criteria);

        return query.ToDictionary(entity => entity.GetId());
    }

    public Task DeleteManyAsync(
        IEnumerable<TKey> ids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        foreach (var id in ids.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            session.Delete<TEntity>(id);
        }

        return Task.CompletedTask;
    }

    public void Add(TEntity entity)
    {
        Insert(entity);
    }

    public void Add(IEnumerable<TEntity> entities)
    {
        Insert(entities);
    }

    public void Insert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        session.Insert(entity);
    }

    public void Insert(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        session.Insert(entities);
    }

    public void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        session.Delete(entity);
    }

    public void Delete(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        session.DeleteObjects(entities);
    }

    public void Upsert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        session.Store(entity);
    }

    public void Upsert(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        session.Store(entities);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return session.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TEntity>> LoadManyAsync(
        IReadOnlyCollection<TKey> ids,
        CancellationToken ct)
    {
        if (typeof(TKey) == typeof(string))
            return await session.LoadManyAsync<TEntity>(ct, ids.Cast<string>());

        if (typeof(TKey) == typeof(Guid))
            return await session.LoadManyAsync<TEntity>(ct, ids.Cast<Guid>());

        if (typeof(TKey) == typeof(int))
            return await session.LoadManyAsync<TEntity>(ct, ids.Cast<int>());

        if (typeof(TKey) == typeof(long))
            return await session.LoadManyAsync<TEntity>(ct, ids.Cast<long>());

        var result = new List<TEntity>(ids.Count);
        foreach (var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            var document = await session.LoadAsync<TEntity>(id, ct);
            if (document is not null) result.Add(document);
        }

        return result;
    }

    private static IQueryable<TEntity> ApplyCriteria(
        IQueryable<TEntity> query,
        Criteria<TEntity>? criteria)
    {
        ValidateCriteria(criteria);
        if (criteria is null) return query;

        foreach (var predicate in criteria.Wheres)
            query = query.Where(predicate);

        if (criteria.OrderBy is not null)
            query = criteria.OrderBy(query);

        if (!criteria.Size.HasValue) return query;

        if (criteria.Page.HasValue)
            query = query.Skip(criteria.Page.Value * criteria.Size.Value);

        return query.Take(criteria.Size.Value);
    }

    private static void ValidateCriteria(Criteria<TEntity>? criteria)
    {
        if (criteria is null) return;

        if (criteria.Includes.Count != 0)
            throw new NotSupportedException(
                $"{nameof(Criteria<>.Includes)} are not supported by Marten document repositories.");

        if (criteria.Track)
            throw new NotSupportedException(
                "Per-query tracking is not supported by Marten document repositories.");

        if (criteria.ForUpdate || criteria.SkipLocked)
            throw new NotSupportedException(
                "ForUpdate and SkipLocked are not supported by Marten document repositories.");
    }
}
