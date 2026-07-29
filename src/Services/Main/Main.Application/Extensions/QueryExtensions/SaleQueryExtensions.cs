using Main.Entities.Sale;
using Main.Enums;

namespace Main.Application.Extensions.QueryExtensions;

public static class SaleQueryExtensions
{
    public static IQueryable<SaleContent> CompletedSales(
        this IQueryable<SaleContent> queryable)
        => queryable.Where(x => x.Sale.State == SaleState.Completed);
}