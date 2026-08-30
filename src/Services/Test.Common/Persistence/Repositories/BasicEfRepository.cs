using Domain;
using Persistence.Interfaces;
using Persistence.Repository;
using Tests.Persistence.Context;

namespace Tests.Persistence.Repositories;

internal sealed class BasicEfRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
	: BasicEfRepositoryBase<DContext, TEntity, TKey>(context, extensions)
	where TEntity : Entity<TEntity, TKey> where TKey : notnull;
