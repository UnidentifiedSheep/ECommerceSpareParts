using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace Persistence.Repository;

internal static class LinqKeyExpressions
{
	public static Expression<Func<TEntity, bool>> BuildKeysPredicate<TEntity, TKey>(IEnumerable<TKey> ids)
		where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
		where TKey : notnull
	{
		var keys = ids.Distinct().ToList();

		return typeof(ICompositeKey).IsAssignableFrom(typeof(TKey))
			? BuildEqualityDisjunction<TEntity, TKey>(keys)
			: BuildContains<TEntity, TKey>(keys);
	}

	private static Expression<Func<TEntity, bool>> BuildContains<TEntity, TKey>(IReadOnlyCollection<TKey> keys)
		where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
		where TKey : notnull
	{
		var keySelector = TEntity.GetKeySelector();
		var contains = Expression.Call(
			typeof(Enumerable),
			nameof(Enumerable.Contains),
			[typeof(TKey)],
			Expression.Constant(keys),
			keySelector.Body);

		return Expression.Lambda<Func<TEntity, bool>>(contains, keySelector.Parameters);
	}

	private static Expression<Func<TEntity, bool>> BuildEqualityDisjunction<TEntity, TKey>(
		IReadOnlyCollection<TKey> keys)
		where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
		where TKey : notnull
	{
		var parameter = Expression.Parameter(typeof(TEntity), "entity");

		var body = keys
			.Select(key =>
			{
				var equality = TEntity.GetEqualityExpression(key);
				return ReplacingExpressionVisitor.Replace(equality.Parameters[0], parameter, equality.Body);
			})
			.DefaultIfEmpty(Expression.Constant(false))
			.Aggregate(Expression.OrElse);

		return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
	}
}
