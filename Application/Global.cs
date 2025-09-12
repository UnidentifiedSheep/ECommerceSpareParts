using System.Text.Json;

namespace Application;

public static class Global
{
    public const string SystemId = "SYSTEM";
    public const int UsdId = 3;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}