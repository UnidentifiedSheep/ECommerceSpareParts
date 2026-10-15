namespace Contracts.Products;

public record ProductCharacteristicsUpdatedEvent
{
	public required DateTime UpdatedAt { get; init; }
	public required IReadOnlyList<ProductCharacteristicsSnapshot> Products { get; init; }
}
