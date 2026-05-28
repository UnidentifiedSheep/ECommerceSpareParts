using Abstractions.Interfaces.Services;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Repository;

public abstract class EfRepository<TEntity, TKey, TContext>(TContext context, IQueryableExtensions extensions)
    : RepositoryBase<TContext, TEntity, TKey>(context, extensions)
    where TEntity : Entity<TEntity, TKey> where TContext : DbContext where TKey : notnull;