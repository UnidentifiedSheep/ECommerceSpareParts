using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public abstract class EfRepository<TEntity, TKey, TContext>(TContext context)
    : RepositoryBase<TContext, TEntity, TKey>(context)
    where TEntity : Entity<TEntity, TKey> where TContext : DbContext;