namespace Application.Common.Extensions;

public static class EnumerableExtensions
{
    public static void EnsureAllExists<T>(
        this IEnumerable<T> ids,
        IEnumerable<T> otherIds,
        Func<IReadOnlyList<T>, Exception> errorFactory)
    {
        var set = ids as HashSet<T> ?? ids.ToHashSet();
        var notFoundIds = new List<T>();

        foreach (var id in otherIds)
            if (!set.Contains(id))
                notFoundIds.Add(id);

        if (notFoundIds.Count > 0) throw errorFactory(notFoundIds);
    }
}