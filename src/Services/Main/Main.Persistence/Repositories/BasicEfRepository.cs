using Domain;
using Main.Persistence.Context;
using Persistence.Repository;

namespace Main.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context)
    : BasicEfRepositoryBase<DContext, TEntity, TKey>(context)
    where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
}