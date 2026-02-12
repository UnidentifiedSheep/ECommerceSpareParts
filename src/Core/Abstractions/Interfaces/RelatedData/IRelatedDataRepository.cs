namespace Abstractions.Interfaces.RelatedData;

public interface IRelatedDataRepository
{
    Task<IEnumerable<string>> GetRelatedDataKeys(string id);
    Task AddRelatedDataAsync(string id, string relatedKey);
    Task AddRelatedDataAsync(IEnumerable<string> ids, string relatedKey);
    Task AddRelatedDataAsync(string id, IEnumerable<string> relatedKeys);
    string GetRelatedDataKey(string id);
}

public interface IRelatedDataRepository<T> : IRelatedDataRepository
{
}