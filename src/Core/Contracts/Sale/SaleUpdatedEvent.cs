using Abstractions.Interfaces;

namespace Contracts.Sale;

public record SaleUpdatedEvent : IKeyedEvent
{
    public required Guid SaleId { get; init; }
    public string GetKey() => $"sale-updated:{SaleId}";
}