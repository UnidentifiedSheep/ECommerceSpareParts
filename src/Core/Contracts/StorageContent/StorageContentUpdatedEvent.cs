namespace Contracts.StorageContent;

public record StorageContentUpdatedEvent
{
    public required int ProductId { get; init; }
}