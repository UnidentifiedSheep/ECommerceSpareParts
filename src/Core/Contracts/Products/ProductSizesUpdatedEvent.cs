using System.Text.Json.Serialization;
using Abstractions.Interfaces;

namespace Contracts.Articles;

public class ProductSizesUpdatedEvent : IKeyedEvent
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    public string GetKey() => $"product-sizes-updated:{ProductId}";
}