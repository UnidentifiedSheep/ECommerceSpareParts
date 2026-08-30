using Domain;

namespace Application.Common.Interfaces.Repositories;

public interface IReadRepository;

public interface IReadRepository<TEntity, TKey> : IReadRepository
	where TEntity : Entity<TEntity, TKey> where TKey : notnull
{
	IQueryable<TEntity> Query { get; }

	Task<IEnumerable<T>> QuerySqlAsync<T>(
		string sql,
		object param,
		CancellationToken cancellationToken = default);
}
