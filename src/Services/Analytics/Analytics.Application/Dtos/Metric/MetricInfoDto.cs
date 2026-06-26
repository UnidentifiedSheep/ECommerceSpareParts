using System.Text.Json.Serialization;

namespace Analytics.Application.Dtos.Metric;

public record MetricInfoDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string Description { get; init; }
    
    [JsonPropertyName("inputSchema")]
    public required string InputSchema { get; init; }
}