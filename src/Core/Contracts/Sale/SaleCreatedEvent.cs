using Contracts.Interfaces;

namespace Contracts.Sale;

public class SaleCreatedEvent(Models.Sale.Sale Sale) : IContract;