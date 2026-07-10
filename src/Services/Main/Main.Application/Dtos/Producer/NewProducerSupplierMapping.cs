using System.Text.Json.Serialization;
using Enums;
using Main.Enums;

namespace Main.Application.Dtos.Producer;

public record NewProducerSupplierMapping
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
    
    [JsonPropertyName("supplier")]
    public required Supplier Supplier { get; init; }
    
    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}