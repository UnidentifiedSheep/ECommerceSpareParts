using Core.Interfaces.CacheRepositories;

namespace Core.Interfaces;

public interface IRelatedDataRepositoryFactory
{
    IRelatedDataRepository<T> GetRepository<T>();
    IRelatedDataRepository GetRepository(Type relatedType);
}