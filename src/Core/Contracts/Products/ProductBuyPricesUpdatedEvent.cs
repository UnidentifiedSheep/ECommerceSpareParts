using Abstractions.Interfaces;

namespace Contracts.Articles;

public class ProductBuyPricesUpdatedEvent : IKeyedEvent
{
    public required int ProductId { get; init; }
    public string GetKey() => $"product-buy-prices-updated:{ProductId}";
}