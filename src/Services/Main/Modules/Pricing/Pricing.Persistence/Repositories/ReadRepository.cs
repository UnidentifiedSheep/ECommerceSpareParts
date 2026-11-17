using Domain;
using Persistence.Context;
using Persistence.Repository;

namespace Pricing.Persistence.Repositories;

public class ReadRepository<TEntity, TKey>(UnifiedDbContext ctx)
	: ReadRepositoryBase<UnifiedDbContext, TEntity, TKey>(ctx)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}
