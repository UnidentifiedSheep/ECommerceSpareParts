using System.Text.Json.Serialization;

namespace Main.Modules.Storage.Application.Dtos.Storage;

public record StorageContentDto
{
	[JsonPropertyName("id")]
	public required int Id { get; init; }

	[JsonPropertyName("storageCode")]
	public required string StorageCode { get; init; }

	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("count")]
	public required int Count { get; init; }

	[JsonPropertyName("buyPrice")]
	public required decimal BuyPrice { get; init; }

	[JsonPropertyName("purchaseDatetime")]
	public required DateTime PurchaseDatetime { get; init; }

	[JsonPropertyName("rowVersion")]
	public required uint RowVersion { get; init; }

	[JsonPropertyName("currencyId")]
	public required int CurrencyId { get; init; }
}
