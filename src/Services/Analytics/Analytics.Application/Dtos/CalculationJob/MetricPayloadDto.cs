using System.Text.Json.Serialization;

namespace Analytics.Application.Dtos.CalculationJob;

public record MetricPayloadDto
{
    //Fields for all metrics.
    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }
    
    [JsonPropertyName("rangeStart")]
    public required DateTime RangeStart { get; init; }
    
    [JsonPropertyName("rangeEnd")]
    public required DateTime RangeEnd { get; init; }

    //Fields based on product
    [JsonPropertyName("productId")]
    public int? ProductId { get; init; }
}