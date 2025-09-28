using Core.Abstractions;
using Core.Interfaces.CacheRepositories;

namespace Core.Interfaces;

public interface IRelatedDataFactory
{
    RelatedDataBase<T> GetRepository<T>();
    Abstractions.RelatedDataBase GetRepository(Type relatedType);
}