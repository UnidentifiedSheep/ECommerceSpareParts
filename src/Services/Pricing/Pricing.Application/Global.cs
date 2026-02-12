using System.Text.Json;

namespace Pricing.Application;

public static class Global
{
    public const int UsdId = 1;
    
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}