using System.Text.Json.Serialization;
using Main.Application.Dtos.Product;

namespace Main.Application.Dtos.Sale;

public record SaleContentDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; set; }

	[JsonPropertyName("discountedPrice")]
	public required decimal DiscountedPrice { get; init; }

	[JsonPropertyName("totalSum")]
	public required decimal TotalSum { get; init; }

	[JsonPropertyName("comment")]
	public string? Comment { get; init; }

	[JsonPropertyName("originalPrice")]
	public required decimal OriginalPrice { get; init; }

	[JsonPropertyName("product")]
	public required ProductDto Product { get; init; }
}
