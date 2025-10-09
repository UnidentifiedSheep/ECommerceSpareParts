using Core.Abstractions;
using Core.Interfaces.CacheRepositories;

namespace Core.Interfaces;

public interface IRelatedDataFactory
{
    RelatedDataBase<T> GetRepository<T>();
    RelatedDataBase GetRepository(Type relatedType);
}