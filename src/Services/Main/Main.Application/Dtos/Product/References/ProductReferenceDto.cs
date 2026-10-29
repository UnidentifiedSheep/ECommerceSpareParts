using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Product.References;

public record ProductReferenceDto
{
	[JsonPropertyName("sku")]
	public required string Sku { get; init; }

	[JsonPropertyName("producerName")]
	public required string ProducerName { get; init; }

	[JsonPropertyName("supplier")]
	public Supplier? Supplier { get; init; }
}
