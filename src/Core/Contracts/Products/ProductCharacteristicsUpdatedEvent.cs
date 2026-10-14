namespace Contracts.Products;

public record ProductCharacteristicsUpdatedEvent
{
	public required IReadOnlyList<int> Ids { get; init; }
}
