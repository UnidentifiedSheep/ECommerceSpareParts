using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Uploads;

public record FileDto
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }
    
    [JsonPropertyName("size")]
    public required long Size { get; init; }
    
    [JsonPropertyName("lastModified")]
    public required DateTime? LastModified { get; init; }
    
    [JsonPropertyName("fullPath")]
    public required string FullPath { get; init; }
}