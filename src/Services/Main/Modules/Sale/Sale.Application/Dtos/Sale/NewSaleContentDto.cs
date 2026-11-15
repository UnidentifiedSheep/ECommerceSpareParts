using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Sale;

public record NewSaleContentDto
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }

	[JsonPropertyName("originalPrice")]
	public required decimal OriginalPrice { get; init; }

	[JsonPropertyName("discountedPrice")]
	public required decimal DiscountedPrice { get; init; }

	[JsonPropertyName("comment")]
	public string? Comment { get; init; }
}
