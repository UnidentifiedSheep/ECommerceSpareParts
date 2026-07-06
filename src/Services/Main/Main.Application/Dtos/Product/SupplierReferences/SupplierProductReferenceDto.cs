using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product.SupplierReferences;

public record SupplierProductReferenceDto
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }
    
    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}