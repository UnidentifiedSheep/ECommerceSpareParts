using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record SupplierProductReferenceDto
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }
    
    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}