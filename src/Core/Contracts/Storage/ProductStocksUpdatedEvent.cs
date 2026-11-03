using System.Text.Json.Serialization;

namespace Contracts.Storage;

public record ProductStocksUpdatedEvent
{
	public required string StorageCode { get; init; }

	public required IReadOnlyList<ProductStockItem> Items { get; init; }

	public required DateTime OccurredAt { get; init; }
}

public record ProductStockItem
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("quantity")]
	public required int Quantity { get; init; }
}
