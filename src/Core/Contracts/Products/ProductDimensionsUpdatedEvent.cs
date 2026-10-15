namespace Contracts.Products;

public record ProductDimensionsUpdatedEvent
{
	public required DateTime UpdatedAt { get; init; }
	public required IReadOnlyList<ProductDimensionsSnapshot> Products { get; init; }
}
