namespace Core.Interfaces.CacheRepositories;

public interface IRelatedDataRepository
{
    Task<IEnumerable<string>> GetRelatedDataKeys(string id);
    Task AddRelatedDataAsync(string id, string relatedKey);
    Task AddRelatedDataAsync(string id, IEnumerable<string> relatedKeys);
}

public interface IRelatedDataRepository<TEntity> : IRelatedDataRepository
{
}