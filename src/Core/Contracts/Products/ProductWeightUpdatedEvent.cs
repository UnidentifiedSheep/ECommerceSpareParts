using System.Text.Json.Serialization;
using Abstractions.Interfaces;

namespace Contracts.Articles;

public class ProductWeightUpdatedEvent : IKeyedEvent
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }
    public string GetKey() => $"product-weight-updated:{ProductId}";
}