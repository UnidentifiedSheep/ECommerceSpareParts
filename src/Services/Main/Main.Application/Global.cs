using System.Text.Json;

namespace Main.Application;

public static class Global
{
    public const int UsdId = 3;
    public static readonly Guid SystemId = Guid.Parse("b4cc24bf-2593-48c3-acf6-f1a87e9af975");

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}