using Domain;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
    : BasicEfRepositoryBase<DContext, TEntity, TKey>(context, extensions)
    where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
}