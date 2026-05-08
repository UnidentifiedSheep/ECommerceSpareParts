using System.Runtime.CompilerServices;
using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Persistence;

public abstract class RepositoryBase<TContext, TEntity, TKey>(TContext context) : IRepository<TEntity, TKey>
    where TEntity : Entity<TEntity, TKey> where TContext : DbContext where TKey : notnull
{
    protected readonly TContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public async ValueTask<TEntity?> GetById(TKey id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(ToKeyValues(id), ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (criteria != null)
            query = query.Apply(criteria);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<TEntity>> ListAsync(
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (criteria != null)
            query = query.Apply(criteria);

        return await query.ToListAsync(ct);
    }

    public abstract Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default);

    private static object[] ToKeyValues(TKey key)
    {
        return key switch
        {
            null => [],
            ITuple v => GetTupleValues(v),
            ICompositeKey k => k.ToArray(), 
            _ => [key]
        };
    }
    
    
    private static object[] GetTupleValues(ITuple tuple)
    {
        var values = new object[tuple.Length];

        for (int i = 0; i < tuple.Length; i++)
            values[i] = tuple[i]!;

        return values;
    }
}