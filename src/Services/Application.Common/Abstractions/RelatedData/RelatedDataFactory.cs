using Abstractions.Interfaces.RelatedData;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Abstractions.RelatedData;

public class RelatedDataFactory(IServiceProvider serviceProvider) : IRelatedDataFactory
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
        if (serviceProvider.GetService(repoType) is not IRelatedDataRepository repo)
            throw new InvalidOperationException($"Repository for {relatedType.Name} not registered");
        return repo;
    }
}