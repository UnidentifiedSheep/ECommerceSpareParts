using Contracts.Interfaces;

namespace Contracts.Sale;

public record SaleCreatedEvent(Models.Sale.Sale Sale) : IContract;