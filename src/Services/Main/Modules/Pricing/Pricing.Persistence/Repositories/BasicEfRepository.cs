using Domain;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Pricing.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(UnifiedDbContext context, IQueryableExtensions extensions)
	: BasicEfRepositoryBase<UnifiedDbContext, TEntity, TKey>(context, extensions)
	where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
}
