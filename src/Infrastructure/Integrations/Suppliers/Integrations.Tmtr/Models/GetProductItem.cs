using System.Text.Json.Serialization;

namespace Integrations.Tmtr.Models;

public record GetProductItem
{
	[JsonPropertyName("article")]
	public required string Number { get; init; }

	[JsonPropertyName("brand")]
	public required string Brand { get; init; }

	[JsonPropertyName("rating")]
	public required int Rating { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }
}
