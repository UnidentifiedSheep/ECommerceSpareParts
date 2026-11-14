using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record ProductGroupDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("normalizedName")]
	public required string NormalizedName { get; init; }
}
