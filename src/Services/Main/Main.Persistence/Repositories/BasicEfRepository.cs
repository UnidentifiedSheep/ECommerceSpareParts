using Domain;
using Main.Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Main.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
    : BasicEfRepositoryBase<DContext, TEntity, TKey>(context, extensions)
    where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
}