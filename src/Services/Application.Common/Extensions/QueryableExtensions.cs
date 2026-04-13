using Abstractions.Models;

namespace Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TModel> ApplyPagination<TModel>(
        this IQueryable<TModel> queryable,
        PaginationModel pagination)
    {
        return queryable.Skip(pagination.Page * pagination.Size).Take(pagination.Size);
    }
}