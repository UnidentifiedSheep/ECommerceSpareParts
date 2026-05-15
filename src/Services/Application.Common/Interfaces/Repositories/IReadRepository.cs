using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IReadRepository<TEntity, TKey> where TEntity : Entity<TEntity, TKey> 
    where TKey : notnull
{
    IQueryable<TEntity> Query { get; }

    Task<IEnumerable<T>> QuerySqlAsync<T>(
        string sql,
        object param);
}