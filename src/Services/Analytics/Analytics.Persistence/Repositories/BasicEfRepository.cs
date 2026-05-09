using Analytics.Persistence.Context;
using Domain;
using Persistence.Repository;

namespace Analytics.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context) 
    : BasicEfRepositoryBase<DContext, TEntity, TKey>(context) 
    where TKey : notnull 
    where TEntity : Entity<TEntity, TKey>
{
    
}