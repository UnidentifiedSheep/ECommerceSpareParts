using Domain;
using Persistence.Repository;
using Tests.Persistence.Context;

namespace Tests.Persistence.Repositories;

internal sealed class ReadRepository<TEntity, TKey>(DContext context)
	: ReadRepositoryBase<DContext, TEntity, TKey>(context)
	where TEntity : Entity<TEntity, TKey> where TKey : notnull;
