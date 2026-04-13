using Domain;
using Main.Persistence.Context;
using Persistence;

namespace Main.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context) 
    : EfRepository<TEntity, TKey, DContext>(context) where TEntity : Entity<TEntity, TKey>
{
    
}