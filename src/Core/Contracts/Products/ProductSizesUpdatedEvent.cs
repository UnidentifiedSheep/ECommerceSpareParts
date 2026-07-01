using System.Text.Json.Serialization;
using Abstractions.Interfaces;

namespace Contracts.Products;

public class ProductSizesUpdatedEvent : IKeyedEvent
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    public string GetKey() { return $"product-sizes-updated:{ProductId}"; }
}