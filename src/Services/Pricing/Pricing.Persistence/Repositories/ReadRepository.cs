using Domain;
using Persistence.Repository;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence.Repositories;

public class ReadRepository<TEntity, TKey>(DContext ctx)
    : ReadRepositoryBase<DContext, TEntity, TKey>(ctx)
    where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}