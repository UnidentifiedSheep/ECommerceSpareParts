using System.Text.Json.Serialization;
using Enums;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalProductReference
{
	[JsonPropertyName("sku")]
	public required string Sku { get; init; }

	[JsonPropertyName("producerName")]
	public required string ProducerName { get; init; }

	[JsonPropertyName("supplier")]
	public Supplier? Supplier { get; init; }
}
