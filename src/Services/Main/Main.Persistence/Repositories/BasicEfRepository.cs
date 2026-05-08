using Application.Common.Interfaces.Repositories;
using Domain;
using Main.Persistence.Context;
using Persistence;

namespace Main.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context)
    : EfRepository<TEntity, TKey, DContext>(context)
    where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
    public override Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        throw new InvalidOperationException("Find by ids is not implemented in BasicEfRepository.");
    }
}