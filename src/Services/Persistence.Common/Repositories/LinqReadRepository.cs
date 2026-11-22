using Domain;
using Domain.Interfaces;
using Persistence.Context;
using Persistence.Repository;

namespace Persistence.Common.Repositories;

public class LinqReadRepository<TEntity, TKey>(UnifiedDbContext ctx)
	: LinqReadRepositoryBase<UnifiedDbContext, TEntity, TKey>(ctx)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>;
