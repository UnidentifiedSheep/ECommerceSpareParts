using Contracts.Interfaces;

namespace Contracts.Sale;

public record SaleDeletedEvent(Models.Sale.Sale Sale) : IContract;