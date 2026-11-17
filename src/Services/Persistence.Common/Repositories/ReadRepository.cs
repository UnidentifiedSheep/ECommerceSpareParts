using Domain;
using Persistence.Context;
using Persistence.Repository;

namespace Persistence.Common.Repositories;

public class ReadRepository<TEntity, TKey>(UnifiedDbContext ctx)
	: ReadRepositoryBase<UnifiedDbContext, TEntity, TKey>(ctx)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}
