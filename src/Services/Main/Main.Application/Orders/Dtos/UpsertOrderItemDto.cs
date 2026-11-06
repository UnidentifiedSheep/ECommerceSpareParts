using System.Text.Json.Serialization;

namespace Main.Application.Orders.Dtos;

public record UpsertOrderItemDto
{
	[JsonPropertyName("id")]
	public int? Id { get; init; }

	[JsonPropertyName("productId")]
	public int ProductId { get; init; }

	[JsonPropertyName("count")]
	public int Count { get; init; }

	[JsonPropertyName("unitPrice")]
	public decimal? UnitPrice { get; init; }
}
