using Abstractions.Models;

namespace Application.Common.Extensions;

public static class EnumerableExtensions
{
    public static void EnsureAllExists<T>(
        this IEnumerable<T> ids,
        IEnumerable<T> otherIds,
        Func<IReadOnlyList<T>, Exception> errorFactory)
    {
        var set = ids as HashSet<T> ?? ids.ToHashSet();
        var notFoundIds = set.Except(otherIds).ToList();

        if (notFoundIds.Count > 0) throw errorFactory(notFoundIds);
    }

    public static IEnumerable<TModel> ApplyPagination<TModel>(
        this IEnumerable<TModel> queryable,
        Pagination pagination)
    {
        return queryable.Skip(pagination.Page * pagination.Size).Take(pagination.Size);
    }
}