using Abstractions.Interfaces;

namespace Contracts.Sale;

public record SaleDeletedEvent : IKeyedEvent
{
    public Guid Id { get; init; }
    public string GetKey() => $"sale-deleted:{Id}";
}