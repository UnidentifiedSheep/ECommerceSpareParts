using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repository;

public abstract class LinqReadRepositoryBase<TContext, TEntity, TKey>(TContext ctx)
	: ReadRepositoryBase<TContext, TEntity, TKey>(ctx)
	where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
	where TKey : notnull
	where TContext : DbContext
{
	public override IQueryable<TEntity> QueryByIds(IEnumerable<TKey> ids)
		=> Query.Where(LinqKeyExpressions.BuildKeysPredicate<TEntity, TKey>(ids));
}
