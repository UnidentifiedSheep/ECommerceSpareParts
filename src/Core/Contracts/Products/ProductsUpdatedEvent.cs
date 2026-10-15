namespace Contracts.Products;

public record ProductsUpdatedEvent
{
	public required DateTime UpdatedAt { get; init; }
	public required IReadOnlyList<ProductSnapshot> Products { get; init; }
}
