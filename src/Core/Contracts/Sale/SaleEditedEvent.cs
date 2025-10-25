using Contracts.Interfaces;

namespace Contracts.Sale;

public record SaleEditedEvent(Models.Sale.Sale Sale, IEnumerable<int> DeletedSaleContents) : IContract;