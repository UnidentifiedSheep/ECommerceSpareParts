using System.Text.Json.Serialization;

namespace Main.Modules.Product.Application.Dtos.Product;

public record CharacteristicDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("normalizedName")]
	public required string NormalizedName { get; init; }
}
