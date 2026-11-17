using Main.Modules.Sale.Abstractions.Models;

namespace Main.Modules.Sale.Abstractions.Interfaces;

public interface ISaleContentSource
{
	IQueryable<SaleContentRef> Query { get; }
}
