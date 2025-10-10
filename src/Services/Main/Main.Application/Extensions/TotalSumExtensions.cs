using Main.Core.Dtos.Amw.Purchase;
using Main.Core.Dtos.Amw.Sales;

namespace Main.Application.Extensions;

public static class TotalSumExtensions
{
    public static decimal GetTotalSum(this IEnumerable<EditPurchaseDto> content)
    {
        return content.Sum(x => x.Count * x.Price);
    }

    public static decimal GetTotalSum(this IEnumerable<NewPurchaseContentDto> content)
    {
        return content.Sum(x => x.Count * x.Price);
    }

    public static decimal GetTotalSum(this IEnumerable<NewSaleContentDto> content)
    {
        return content.Sum(x => x.Count * x.PriceWithDiscount);
    }

    public static decimal GetTotalSum(this IEnumerable<EditSaleContentDto> content)
    {
        return content.Sum(x => x.Count * x.Price);
    }
}