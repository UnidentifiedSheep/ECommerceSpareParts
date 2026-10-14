using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record UpsertCharacteristicsDto
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("characteristicId")]
	public required int CharacteristicId { get; init; }

	[JsonPropertyName("value")]
	public required string Value { get; init; }
}
