using Domain;
using Domain.Interfaces;
using Persistence.Interfaces;
using Persistence.Repository;
using Tests.Persistence.Context;

namespace Tests.Persistence.Repositories;

internal sealed class LinqRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, TEntity, TKey>(context, extensions)
	where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey> where TKey : notnull;
