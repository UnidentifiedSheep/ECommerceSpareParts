using Application.Common.Interfaces.Repositories;
using Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Services.Persistence;

public sealed class RepositoryProvider(
    IServiceProvider serviceProvider
    ) : IRepositoryProvider
{
    public IRepository<TEntity, TKey> Get<TEntity, TKey>()
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        return serviceProvider.GetRequiredService<IRepository<TEntity, TKey>>();
    }

    public IReadRepository<TEntity, TKey> GetForRead<TEntity, TKey>()
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        return serviceProvider.GetRequiredService<IReadRepository<TEntity, TKey>>();
    }

    public TRepository Get<TRepository>()
        where TRepository : class, IRepository
    {
        return serviceProvider.GetRequiredService<TRepository>();
    }
}
