using Abstractions.Interfaces;
using Contracts.Sale.Model;

namespace Contracts.Sale;

public record SaleUpdatedEvent : IKeyedEvent
{
    public required SaleEventModel Sale { get; init; }
    public required int BaseCurrencyId { get; init; }
    public required DateTime OccurredAt { get; init; }
    public string GetKey() => $"sale-updated:{Sale.Id}";
}