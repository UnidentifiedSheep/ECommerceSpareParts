using Domain;
using Domain.Interfaces;
using Main.Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories;

public class BasicLinqRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
	: LinqRepositoryBase<DContext, TEntity, TKey>(context, extensions)
	where TKey : notnull where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>;
