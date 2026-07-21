using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Producer.SupplierMappings;

public record ProducerSupplierMappingDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }
    
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }
    
    [JsonPropertyName("supplier")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Supplier Supplier { get; init; }
    
    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}