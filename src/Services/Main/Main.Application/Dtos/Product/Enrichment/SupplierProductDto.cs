using System.Text.Json.Serialization;
using Enums;

namespace Main.Application.Dtos.Product.Enrichment;

public record SupplierProductDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("sku")]
	public required string Sku { get; init; }

	[JsonPropertyName("producer")]
	public required string Producer { get; init; }

	[JsonPropertyName("supplier")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required Supplier Supplier { get; init; }

	[JsonPropertyName("names")]
	public required IReadOnlyList<SupplierProductNameDto> Names { get; init; }
}
