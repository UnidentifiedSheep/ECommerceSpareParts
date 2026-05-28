using Abstractions.Interfaces.Services;
using Analytics.Persistence.Context;
using Domain;
using Persistence.Interfaces;
using Persistence.Repository;

namespace Analytics.Persistence.Repositories;

public class BasicEfRepository<TEntity, TKey>(DContext context, IQueryableExtensions extensions)
    : BasicEfRepositoryBase<DContext, TEntity, TKey>(context, extensions)
    where TKey : notnull
    where TEntity : Entity<TEntity, TKey>
{
}