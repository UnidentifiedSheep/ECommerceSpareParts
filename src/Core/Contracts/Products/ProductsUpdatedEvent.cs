namespace Contracts.Products;

public record ProductsUpdatedEvent
{
	public required IReadOnlyList<int> Ids { get; init; }
}
