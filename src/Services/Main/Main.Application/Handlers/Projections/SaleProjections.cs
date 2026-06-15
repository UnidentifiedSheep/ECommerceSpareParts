using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Sale;
using Main.Entities.Currency;
using Main.Entities.Sale;

namespace Main.Application.Handlers.Projections;

public static class SaleProjections
{
    public static readonly Expression<Func<Sale, SaleDto>> ToSaleDto =
        x => new SaleDto
        {
            Id = x.Id,
            Buyer = UserProjections.UserProjection.Invoke(x.Buyer),
            Comment = x.Comment,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency),
            SaleDatetime = x.SaleDatetime,
            Storage = x.StorageName,
            TotalSum = x.Transaction.Amount,
            TransactionId = x.TransactionId
        };

    public static readonly Expression<Func<SaleContent, SaleContentDto>> ToSaleContentDto =
        x => new SaleContentDto
        {
            Id = x.Id,
            Count = x.Count,
            Price = x.Price,
            TotalSum = x.TotalSum,
            Discount = x.Discount,
            Comment = x.Comment,
            Product = ProductProjections.ToDto.Invoke(x.Product),
            Details = x.Details.Select(z => ToSaleContentDetailDto.Invoke(z)).ToList()
        };

    public static readonly Expression<Func<SaleContentDetail, SaleContentDetailDto>> ToSaleContentDetailDto =
        x => new SaleContentDetailDto
        {
            Id = x.Id,
            BuyPrice = x.BuyPrice,
            Count = x.Count,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency),
            PurchaseDatetime = x.PurchaseDatetime,
            SaleContentId = x.SaleContentId,
            StorageContentId = x.StorageContentId
        };
}