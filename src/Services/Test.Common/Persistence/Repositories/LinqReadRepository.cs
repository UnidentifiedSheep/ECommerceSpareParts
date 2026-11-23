using Domain;
using Domain.Interfaces;
using Persistence.Repository;
using Tests.Persistence.Context;

namespace Tests.Persistence.Repositories;

internal sealed class LinqReadRepository<TEntity, TKey>(DContext context)
	: LinqReadRepositoryBase<DContext, TEntity, TKey>(context)
	where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey> where TKey : notnull;
