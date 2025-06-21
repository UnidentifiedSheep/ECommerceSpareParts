namespace Core.StaticFunctions;

public static class StaticString
{
    public static bool IsAnyMatchInvariant(this IEnumerable<string> collection, params string[] values)
    {
        if (values.Length == 0) return false;
        var valueSet = new HashSet<string>(values, StringComparer.InvariantCultureIgnoreCase);
        return valueSet.Count != 0 && collection.Any(item => valueSet.Contains(item));
    }
}