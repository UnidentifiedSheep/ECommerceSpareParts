using System.Text.Json.Serialization;

namespace Main.Modules.Cart.Application.Dtos.Cart;

public record CartItemDto
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }

	[JsonPropertyName("createdAt")]
	public required DateTime CreatedAt { get; init; }

	[JsonPropertyName("updatedAt")]
	public required DateTime? UpdatedAt { get; init; }
}
