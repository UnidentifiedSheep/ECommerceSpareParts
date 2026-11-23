using Main.Modules.Sale.Application.Models;

namespace Main.Modules.Sale.Application.Interfaces.Services;

public interface ISalesFilterApplier
{
	IQueryable<Entities.Sale.Sale.Sale> Apply(IQueryable<Entities.Sale.Sale.Sale> query, SalesFilter filter);
}
