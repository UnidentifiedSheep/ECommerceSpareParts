using Core.Dtos.Amw.Purchase;
using Core.Dtos.Amw.Sales;

namespace Application.Extensions;

public static class TotalSumExtensions
{
    public static decimal GetTotalSum(this IEnumerable<EditPurchaseDto> content) => content.Sum(x => x.Count * x.Price);
    public static decimal GetTotalSum(this IEnumerable<NewPurchaseContentDto> content) => content.Sum(x => x.Count * x.Price);
    public static decimal GetTotalSum(this IEnumerable<NewSaleContentDto> content) => content.Sum(x => x.Count * x.PriceWithDiscount);
    public static decimal GetTotalSum(this IEnumerable<EditSaleContentDto> content) => content.Sum(x => x.Count * x.Price);
}