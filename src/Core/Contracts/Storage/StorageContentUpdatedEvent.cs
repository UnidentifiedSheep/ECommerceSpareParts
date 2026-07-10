using Abstractions.Interfaces.Events;

namespace Contracts.Storage;

public record StorageContentUpdatedEvent : IKeyedEvent
{
    public required DateTime OccurredAt { get; init; }
    public required string StorageName { get; init; }
    public required int ProductId { get; init; }
    public required int StorageContentId { get; init; }
    public required int CurrencyId { get; init; }
    public required decimal BuyPrice { get; init; }
    public required int AvailableCount { get; init; }
    public string GetKey() => $"storage:content:{StorageContentId}:updated";
}