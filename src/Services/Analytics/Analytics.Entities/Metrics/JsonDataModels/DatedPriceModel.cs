using System.Text.Json.Serialization;

namespace Analytics.Entities.Metrics.JsonDataModels;

public record DatedPriceModel
{
    [JsonPropertyName("date")]
    public required DateTime Date { get; init; }
    
    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
    
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
}