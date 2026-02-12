namespace Abstractions.Interfaces.RelatedData;

public interface IRelatedDataFactory
{
    IRelatedDataRepository<T> GetRepository<T>();
    IRelatedDataRepository GetRepository(Type relatedType);
}