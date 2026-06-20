using System.Text.Json.Serialization;
using Analytics.Enums;

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
    
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required RecalculationTags Tags { get; init; }
    
    [JsonPropertyName("rangeStart")]
    public required DateTime RangeStart { get; init; }
    
    [JsonPropertyName("rangeEnd")]
    public required DateTime RangeEnd { get; init; }
    
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("productId")]
    public required int? ProductId { get; init; }
    
    [JsonPropertyName("lastMetricJob")]
    public required MetricJobDto? LastMetricJob { get; init; }
}