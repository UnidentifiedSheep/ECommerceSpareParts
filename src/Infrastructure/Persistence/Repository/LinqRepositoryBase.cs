using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Repository;

public abstract class LinqRepositoryBase<TContext, TEntity, TKey>(
	TContext context,
	IQueryableExtensions extensions) : RepositoryBase<TContext, TEntity, TKey>(context, extensions)
	where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
	where TKey : notnull
	where TContext : DbContext
{
	public override Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
		IEnumerable<TKey> ids,
		Criteria<TEntity>? criteria = null,
		CancellationToken ct = default)
		=> QueryableExtensions
			.Apply(DbSet.AsQueryable(), criteria)
			.Where(LinqKeyExpressions.BuildKeysPredicate<TEntity, TKey>(ids))
			.ToDictionaryAsync(TEntity.GetKeySelector().Compile(), ct);

	public override Task DeleteManyAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
		=> DbSet.AsQueryable()
			.Where(LinqKeyExpressions.BuildKeysPredicate<TEntity, TKey>(ids))
			.ExecuteDeleteAsync(cancellationToken);
}
