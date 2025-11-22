using System.Text.Json;

namespace Main.Application;

public static class Global
{
    public const int UsdId = 3;
    public static Guid SystemId { get; private set; } = Guid.Empty;
    public const string ImageBucketName = "imgs";
    public static string ServiceUrl { get; private set; } = "";

    public static void SetServiceUrl(string url) => ServiceUrl = url;
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void SetSystemId(string id)
    {
        SystemId = Guid.Parse(id);
    }
}