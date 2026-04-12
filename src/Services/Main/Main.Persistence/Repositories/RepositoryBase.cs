using Application.Common.Interfaces.Repositories;
using Domain;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public abstract class RepositoryBase<TEntity, TKey>(DContext context) : IRepository<TEntity, TKey>
    where TEntity : Entity<TEntity, TKey>
{
    protected readonly DContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public async ValueTask<TEntity?> GetById(TKey id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
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
}