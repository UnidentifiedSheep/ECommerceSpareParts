using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Price;

public sealed record PriceRecommendationDto
{
	[JsonPropertyName("quantity")]
	public required int Quantity { get; init; }

	[JsonPropertyName("missingQuantity")]
	public required int MissingQuantity { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }

	[JsonPropertyName("unitPrice")]
	public required decimal? UnitPrice { get; init; }

	[JsonPropertyName("totalPrice")]
	public decimal? TotalPrice => UnitPrice * Quantity;

	[JsonPropertyName("items")]
	public required IReadOnlyList<PriceRecommendationItemDto> Items { get; init; }
}

public sealed record PriceRecommendationItemDto(
	[property: JsonPropertyName("quantity")] int Quantity,
	[property: JsonPropertyName("option")] PriceOptionDto Option);
