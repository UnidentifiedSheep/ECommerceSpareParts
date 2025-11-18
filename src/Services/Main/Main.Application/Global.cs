using System.Text.Json;

namespace Main.Application;

public static class Global
{
    public const int UsdId = 3;
    public static Guid SystemId { get; private set; } = Guid.Empty;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void SetSystemId(string id)
    {
        SystemId = Guid.Parse(id);
    }
}