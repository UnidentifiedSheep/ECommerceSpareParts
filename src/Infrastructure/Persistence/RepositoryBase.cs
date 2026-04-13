using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Persistence;

public abstract class RepositoryBase<TContext, TEntity, TKey>(TContext context) : IRepository<TEntity, TKey>
    where TEntity : Entity<TEntity, TKey> where TContext : DbContext
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

    private static object[] ToKeyValues(TKey key)
    {
        return key switch
        {
            null => [],
            ValueTuple v => GetTupleValues(v),
            ICompositeKey k => k.ToArray(), 
            _ => [key]
        };
    }
    
    private static object[] GetTupleValues(object tuple)
    {
        var type = tuple.GetType();

        var fields = type.GetFields();

        var values = new object[fields.Length];
        for (int i = 0; i < fields.Length; i++)
            values[i] = fields[i].GetValue(tuple)!;

        return values;
    }
}