using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polly;
using Polly.Retry;
using Serilog;

namespace Redis;

public static class Global
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    
    public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
}