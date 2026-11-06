using System.Text.Json.Serialization;
using Main.Enums.Orders;

namespace Main.Application.Orders.Dtos;

public sealed record OrderItemDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("orderId")]
	public required Guid OrderId { get; init; }

	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }

	[JsonPropertyName("unitPrice")]
	public required decimal? UnitPrice { get; init; }

	[JsonPropertyName("totalPrice")]
	public decimal? TotalPrice => UnitPrice * Count;

	[JsonPropertyName("priceOrigin")]
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required PriceOrigin PriceOrigin { get; init; }
}
