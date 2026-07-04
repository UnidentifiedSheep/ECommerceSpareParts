using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalSupplierProductReference
{
    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("supplierProducerName")]
    public required string SupplierProducerName { get; init; }
}
