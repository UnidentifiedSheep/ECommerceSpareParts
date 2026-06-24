using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Extensions;

public static class JsonExtensions
{
    public static bool TryDeserializeJson<T>(
        this string json,
        [NotNullWhen(true)] out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return result is not null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }
}