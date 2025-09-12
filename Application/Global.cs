using System.Text.Json;

namespace Application;

public static class Global
{
    public const string SystemId = "SYSTEM";
    public const int UsdId = 3;
    public readonly static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}