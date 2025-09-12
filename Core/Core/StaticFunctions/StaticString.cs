namespace Core.StaticFunctions;

public static class StaticString
{
    public static bool IsAnyMatchInvariant(this IEnumerable<string> collection, params string[] values)
    {
        if (values.Length == 0) return false;
        var valueSet = new HashSet<string>(values, StringComparer.InvariantCultureIgnoreCase);
        return valueSet.Count != 0 && collection.Any(item => valueSet.Contains(item));
    }

    public static (string? name, string? direction) GetSortNameNDirection(this string? value, char delimiter = '_')
    {
        var sortSplit = value?.Split(delimiter);
        var sortName = sortSplit?.Length > 0 ? sortSplit[0] : null;
        var sortDirection = sortSplit?.Length > 1 ? sortSplit[1] : null;
        return (sortName, sortDirection);
    }
}