using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Keys;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Repository;

public abstract class RepositoryBase<TContext, TEntity, TKey>(
	TContext context,
	IQueryableExtensions queryableExtensions) : IRepository<TEntity, TKey>
	where TEntity : Entity<TEntity, TKey> where TContext : DbContext where TKey : notnull
{
	protected readonly TContext Context = context;

	protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

	protected readonly IQueryableExtensions QueryableExtensions = queryableExtensions;

	public async ValueTask<TEntity?> GetById(TKey id, CancellationToken ct = default) =>
		await DbSet.FindAsync(EntityKey.GetValues(id), ct);

	public async Task<TEntity?> FirstOrDefaultAsync(
		Criteria<TEntity>? criteria = null,
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable();

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return await query.FirstOrDefaultAsync(ct);
	}

	public async Task<List<TEntity>> ListAsync(
		Criteria<TEntity>? criteria = null,
		CancellationToken ct = default)
	{
		var query = DbSet.AsQueryable();

		if (criteria != null)
			query = QueryableExtensions.Apply(query, criteria);

		return await query.ToListAsync(ct);
	}

	public IAsyncEnumerable<TEntity> AsyncEnumerable(Criteria<TEntity>? criteria = null)
	{
		var query = DbSet.AsQueryable();
		query = QueryableExtensions.Apply(query, criteria);
		return query.AsAsyncEnumerable();
	}

	public abstract Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
		IEnumerable<TKey> ids,
		Criteria<TEntity>? criteria = null,
		CancellationToken ct = default);
	public abstract Task DeleteManyAsync(
		IEnumerable<TKey> ids,
		CancellationToken cancellationToken = default);
}
