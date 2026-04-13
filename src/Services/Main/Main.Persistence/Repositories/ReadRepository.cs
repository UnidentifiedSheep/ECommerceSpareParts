using Application.Common.Interfaces.Repositories;
using Domain;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Main.Persistence.Repositories;

public class ReadRepository<TEntity, TKey>(DContext ctx)
    : IReadRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey>
{
    public IQueryable<TEntity> Query => ctx.Set<TEntity>().AsQueryable().AsNoTracking();
}