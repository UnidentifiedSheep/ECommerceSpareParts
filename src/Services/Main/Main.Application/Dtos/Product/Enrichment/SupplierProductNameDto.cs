using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product.Enrichment;

public record SupplierProductNameDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("supplierProductId")]
    public required int SupplierProductId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
