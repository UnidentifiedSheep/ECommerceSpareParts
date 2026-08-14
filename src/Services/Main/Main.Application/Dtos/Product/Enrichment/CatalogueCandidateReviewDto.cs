using System.Text.Json.Serialization;
using Main.Application.Dtos.Producer;

namespace Main.Application.Dtos.Product.Enrichment;

public record CatalogueCandidateReviewDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("producer")]
    public required ProducerDto Producer { get; init; }

    [JsonPropertyName("product")]
    public required ProductDto? Product { get; init; }

    [JsonPropertyName("sku")]
    public required string Sku { get; init; }

    [JsonPropertyName("supplierProducts")]
    public required IReadOnlyList<SupplierProductDto> SupplierProducts { get; init; }
}
