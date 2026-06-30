using System.Text.Json.Serialization;

namespace Application.Common.Dtos;

public record SettingDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string Description { get; init; }
    
    [JsonPropertyName("inputData")]
    public required string InputData { get; init; }
    
    [JsonPropertyName("outputData")]
    public required string OutputData { get; init; }
}