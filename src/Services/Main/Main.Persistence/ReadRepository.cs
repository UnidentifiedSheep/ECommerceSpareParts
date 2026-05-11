using Domain;
using Main.Persistence.Context;
using Persistence.Repository;

namespace Main.Persistence;

public class ReadRepository<TEntity, TKey>(DContext ctx)
    : ReadRepositoryBase<DContext, TEntity, TKey>(ctx)
    where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
}