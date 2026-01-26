using System.Text.Json;

namespace Main.Application;

public static class Global
{
    public const int UsdId = 1;
    public static Guid SystemId { get; private set; } = Guid.Empty;
    public static string ImageBucketName { get; private set; } = "";
    public static string ServiceUrl { get; private set; } = "";

    public static void SetServiceUrl(string url) => ServiceUrl = url;
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    public static void SetImageBucketName(string name) => ImageBucketName = name;

    public static void SetSystemId(string id)
    {
        SystemId = Guid.Parse(id);
    }
}