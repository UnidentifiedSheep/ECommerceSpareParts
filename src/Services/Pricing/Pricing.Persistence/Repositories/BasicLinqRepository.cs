using Domain;
using Domain.Interfaces;
using Persistence.Interfaces;
using Persistence.Repository;
using Pricing.Persistence.Contexts;

namespace Pricing.Persistence.Repositories;

public class BasicLinqRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, TEntity, TKey>(context, extensions)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>;
