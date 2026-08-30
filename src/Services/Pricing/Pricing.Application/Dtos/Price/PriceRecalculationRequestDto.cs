using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Price;

public record PriceRecalculationRequestDto
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }
}
