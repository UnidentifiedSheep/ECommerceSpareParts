using System.Text.Json;

namespace Core.Configs;

public static class JsonConfigs
{
    public static JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}