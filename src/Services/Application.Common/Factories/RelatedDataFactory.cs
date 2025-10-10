using Core.Abstractions;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Factories;

public class RelatedDataFactory(IServiceProvider serviceProvider) : IRelatedDataFactory
{
    public RelatedDataBase<T> GetRepository<T>()
    {
        var repository = serviceProvider.GetRequiredService<RelatedDataBase<T>>();
        if (repository == null)
            throw new InvalidOperationException($"Repository for type {typeof(T).Name} is not registered.");
        return repository;
    }

    public RelatedDataBase GetRepository(Type relatedType)
    {
        var repoType = typeof(RelatedDataBase<>).MakeGenericType(relatedType);
        var repo = serviceProvider.GetService(repoType) as RelatedDataBase;
        if (repo == null)
            throw new InvalidOperationException($"Repository for {relatedType.Name} not registered");
        return repo;
    }
}