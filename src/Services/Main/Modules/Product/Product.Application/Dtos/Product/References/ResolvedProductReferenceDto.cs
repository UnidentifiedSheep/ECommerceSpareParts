using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Product.References;

public record ResolvedProductReferenceDto
{
	[JsonPropertyName("producerId")]
	public required int? ProducerId { get; init; }

	[JsonPropertyName("productId")]
	public required int? ProductId { get; init; }

	[JsonPropertyName("sku")]
	public required string Sku { get; init; }

	[JsonPropertyName("producerName")]
	public required string ProducerName { get; init; }

	[JsonPropertyName("supplier")]
	public Supplier? Supplier { get; init; }
}
