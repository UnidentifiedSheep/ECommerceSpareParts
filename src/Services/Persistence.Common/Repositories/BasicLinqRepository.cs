using Domain;
using Domain.Interfaces;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Persistence.Common.Repositories;

public class BasicLinqRepository<TEntity, TKey>(UnifiedDbContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<UnifiedDbContext, TEntity, TKey>(context, extensions)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>;
