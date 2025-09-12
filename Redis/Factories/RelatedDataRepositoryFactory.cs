using Core.Interfaces;
using Core.Interfaces.CacheRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Redis.Factories;

public class RelatedDataRepositoryFactory(IServiceProvider serviceProvider) : IRelatedDataRepositoryFactory
{
    public IRelatedDataRepository<T> GetRepository<T>()
    {
        var repository = serviceProvider.GetRequiredService<IRelatedDataRepository<T>>();
        if (repository == null)
            throw new InvalidOperationException($"Repository for type {typeof(T).Name} is not registered.");
        return repository;
    }

    public IRelatedDataRepository GetRepository(Type relatedType)
    {
        var repoType = typeof(IRelatedDataRepository<>).MakeGenericType(relatedType);
        var repo = serviceProvider.GetService(repoType) as IRelatedDataRepository;
        if (repo == null)
            throw new InvalidOperationException($"Repository for {relatedType.Name} not registered");
        return repo;
    }
}