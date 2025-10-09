using Core.Abstractions;

namespace Core.Interfaces;

public interface IRelatedDataFactory
{
    RelatedDataBase<T> GetRepository<T>();
    RelatedDataBase GetRepository(Type relatedType);
}