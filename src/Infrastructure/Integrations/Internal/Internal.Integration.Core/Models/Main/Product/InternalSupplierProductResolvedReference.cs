using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalSupplierProductResolvedReference
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}
