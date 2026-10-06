using Contracts.Interfaces;

namespace Contracts.Products;

public record ProductDeletedEvent : IKeyedEvent
{
	public required int Id { get; init; }

	public string GetKey() => $"product-deleted:{Id}";
}
