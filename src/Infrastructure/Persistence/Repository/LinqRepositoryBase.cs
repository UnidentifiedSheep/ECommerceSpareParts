using System.Linq.Expressions;
using Application.Common.Interfaces.Repositories;
using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Persistence.Repository;

public abstract class LinqRepositoryBase<TContext, TEntity, TKey>(TContext context)
    : RepositoryBase<TContext, TEntity, TKey>(context)
    where TEntity : Entity<TEntity, TKey>, ILinqEntity<TEntity, TKey>
    where TKey : notnull
    where TContext : DbContext
{
    public override Task<Dictionary<TKey, TEntity>> FindByIdsAsync(
        IEnumerable<TKey> ids,
        Criteria<TEntity>? criteria = null,
        CancellationToken ct = default)
    {
        var keys = ids.ToList();
        var keySelector = TEntity.GetKeySelector();
        var containsExpression = BuildContainsExpression(keys, keySelector);

        return DbSet
            .AsQueryable()
            .Apply(criteria)
            .Where(containsExpression)
            .ToDictionaryAsync(keySelector.Compile(), ct);
    }

    private static Expression<Func<TEntity, bool>> BuildContainsExpression(
        IReadOnlyCollection<TKey> keys,
        Expression<Func<TEntity, TKey>> keySelector)
    {
        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [typeof(TKey)],
            Expression.Constant(keys),
            keySelector.Body);

        return Expression.Lambda<Func<TEntity, bool>>(contains, keySelector.Parameters);
    }
}
