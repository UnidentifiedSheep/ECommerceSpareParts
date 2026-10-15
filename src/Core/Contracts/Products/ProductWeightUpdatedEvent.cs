namespace Contracts.Products;

public record ProductWeightUpdatedEvent
{
	public required DateTime UpdatedAt { get; init; }
	public required IReadOnlyList<ProductWeightSnapshot> Products { get; init; }
}
