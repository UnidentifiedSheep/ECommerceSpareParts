using Application.Common.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repository;

public abstract class BasicEfRepositoryBase<TContext, TEntity, TKey>(TContext context)
    : EfRepository<TEntity, TKey, TContext>(context)
    where TEntity : Entity<TEntity, TKey> where TKey : notnull 
    where TContext : DbContext
{
    public override Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        throw new InvalidOperationException("Find by ids is not implemented in BasicEfRepository.");
    }
}