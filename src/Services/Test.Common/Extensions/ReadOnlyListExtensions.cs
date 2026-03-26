namespace Test.Common.Extensions;

public static class ReadOnlyListExtensions
{
    public static IReadOnlyList<T> ReplaceAt<T>(
        this IReadOnlyList<T> source,
        int index,
        Func<T, T> replace)
    {
        return source
            .Select((x, i) => i == index ? replace(x) : x)
            .ToList();
    }
}