namespace Contracts.Products;

public record ProductsDeletedEvent
{
	public required IReadOnlyList<int> Ids { get; init; }
}
