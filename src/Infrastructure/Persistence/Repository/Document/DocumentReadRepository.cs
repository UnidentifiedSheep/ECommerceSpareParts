using Application.Common.Interfaces.Repositories;
using Domain;
using Marten;

namespace Persistence.Repository.Document;

public class DocumentReadRepository<TEntity, TKey>(IDocumentSession session)
	: IDocumentReadRepository<TEntity, TKey> where TKey : notnull where TEntity : Entity<TEntity, TKey>
{
	public IQueryable<TEntity> Query => session.Query<TEntity>();

	public IQueryable<TEntity> QueryByIds(IEnumerable<TKey> ids)
		=> throw new InvalidOperationException("Query by ids is not implemented in DocumentReadRepository.");

	public async Task<IEnumerable<T>> QuerySqlAsync<T>(
		string sql,
		object param,
		CancellationToken cancellationToken = default)
		=> await session.QueryAsync<T>(
			sql,
			cancellationToken,
			param);
}
