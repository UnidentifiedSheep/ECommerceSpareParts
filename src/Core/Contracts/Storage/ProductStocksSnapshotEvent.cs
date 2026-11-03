using System.Text.Json.Serialization;

namespace Contracts.Storage;

public record ProductStocksSnapshotEvent
{
	public required DateTime OccurredAt { get; init; }

	public required IReadOnlyList<ProductStocksSnapshot> Products { get; init; }
}

public record ProductStocksSnapshot
{
	[JsonPropertyName("productId")]
	public required int ProductId { get; init; }

	[JsonPropertyName("stocks")]
	public required IReadOnlyDictionary<string, int> Stocks { get; init; }
}
