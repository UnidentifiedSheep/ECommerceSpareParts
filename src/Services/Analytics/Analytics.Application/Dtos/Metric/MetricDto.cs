using System.Text.Json.Serialization;

namespace Analytics.Application.Dtos.Metric;

public record MetricDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public required string Description { get; init; }
    
    [JsonPropertyName("data")]
    public required string? Data { get; init; }
}