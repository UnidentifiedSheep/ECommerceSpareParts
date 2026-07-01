using Abstractions.Interfaces;

namespace Contracts.Sale;

public record SaleDeletedEvent : IKeyedEvent
{
    public Guid SaleId { get; init; }
    public string GetKey() { return $"sale-deleted:{SaleId}"; }
}