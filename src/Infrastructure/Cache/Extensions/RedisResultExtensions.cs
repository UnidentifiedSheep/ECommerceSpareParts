using System.Text.Json;
using StackExchange.Redis;

namespace Cache.Extensions;

public static class RedisResultExtensions
{
    public static T? Deserialize<T>(this RedisResult result)
    {
        if (result.IsNull)
            return default;

        var json = result.ToString();

        if (string.IsNullOrWhiteSpace(json))
            return default;

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;
        
        if (root.ValueKind != JsonValueKind.Array)
            return root.Deserialize<T>();
        
        if (root.GetArrayLength() == 0)
            return default;

        root = root[0];

        return root.Deserialize<T>();
    }

    public static IReadOnlyList<T?> DeserializeMany<T>(this IEnumerable<RedisResult> results)
    {
        return results
            .Select(Deserialize<T>)
            .ToList();
    }
}