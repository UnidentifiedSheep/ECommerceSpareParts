using Main.Entities.Sale.Enums;
using Main.Entities.Sale.Sale;

namespace Main.Modules.Sale.Application.Extensions;

public static class SaleQueryExtensions
{
	public static IQueryable<SaleContent> CompletedSales(this IQueryable<SaleContent> queryable)
		=> queryable.Where(x => x.Sale.State == SaleState.Completed);
}
