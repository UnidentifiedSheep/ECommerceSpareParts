using Persistence.Context;
using Domain;
using Main.Persistence.Context;
using Persistence.Repository;

namespace Main.Persistence;

public class ReadRepository<TEntity, TKey>(UnifiedDbContext ctx) : ReadRepositoryBase<UnifiedDbContext, TEntity, TKey>(ctx)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}
