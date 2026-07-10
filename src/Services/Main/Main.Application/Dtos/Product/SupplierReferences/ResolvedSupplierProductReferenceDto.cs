using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product.SupplierReferences;

public record ResolvedSupplierProductReferenceDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}
