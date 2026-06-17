using Abstractions;
using Abstractions.Models.SortyBy;
using Application.Common.Interfaces.Repositories;

namespace Application.Common.Extensions;

public static class SortByExtensions
{
    public static IQueryable<TEntity> SortBy<TEntity>(
        this IQueryable<TEntity> query,
        string? sortParam)
    {
        var sort = QueryableSortBy.ParseToKeySelector<TEntity>(sortParam);
        
        return sort.Desc
            ? query.OrderByDescending(sort.KeySelector)
            : query.OrderBy(sort.KeySelector);
    }

    public static CriteriaBuilder<TEntity> WithSorting<TEntity>(
        this CriteriaBuilder<TEntity> builder,
        string? sortParam) where TEntity : class
    {
        var sort = QueryableSortBy.ParseToKeySelector<TEntity>(sortParam);

        if (sort.Desc)
            builder.OrderByDesc(sort.KeySelector);
        else
            builder.OrderByAsc(sort.KeySelector);

        return builder;
    }
}