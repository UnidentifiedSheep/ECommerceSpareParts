using Analytics.Persistence.Context;
using Domain;
using Persistence.Repository;

namespace Analytics.Persistence.Repositories;

public class ReadRepository<TEntity, TKey>(DContext ctx)
    : ReadRepositoryBase<DContext, TEntity, TKey>(ctx)
    where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}