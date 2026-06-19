using Abstractions.Interfaces;

namespace Contracts.Sale;

public record SaleDeletedEvent : IKeyedEvent
{
    public Guid SaleId { get; init; }
    public string GetKey() => $"sale-deleted:{SaleId}";
}