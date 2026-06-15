using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Projections;

public static class PurchaseProjections
{
    public static readonly Expression<Func<Purchase, PurchaseDto>> ToPurchaseDto =
        x => new PurchaseDto
        {
            Id = x.Id,
            Comment = x.Comment,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency),
            Logistics = x.PurchaseLogistic == null ? null : ToPurchaseLogisticDto.Invoke(x.PurchaseLogistic),
            PurchaseDatetime = x.PurchaseDatetime,
            Storage = x.Storage,
            Supplier = UserProjections.UserProjection.Invoke(x.Supplier),
            TotalSum = x.Transaction.Amount,
            TransactionId = x.TransactionId
        };
    
    public static readonly Expression<Func<PurchaseLogistic, PurchaseLogisticDto>> ToPurchaseLogisticDto =
        x => new PurchaseLogisticDto
        {
            RouteId = x.RouteId,
            TransactionId = x.TransactionId,
            PricingModel = x.PricingModel,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency),
            MinimumPrice = x.MinimumPrice,
            MinimumPriceApplied = x.MinimumPriceApplied,
            PriceKg = x.PriceKg,
            PricePerM3 = x.PricePerM3,
            PricePerOrder = x.PricePerOrder,
            RouteType = x.RouteType
        };
    
    public static readonly Expression<Func<PurchaseContent, PurchaseContentDto>> ToContentDto =
        x => new PurchaseContentDto
        {
            Id = x.Id,
            Count = x.Count,
            Comment = x.Comment,
            Price = x.Price,
            TotalSum = x.TotalSum,
            Product = ProductProjections.ToDto.Invoke(x.Product),
            ContentLogistics = x.PurchaseContentLogistic == null 
                ? null 
                : ToContentLogisticDto.Invoke(x.PurchaseContentLogistic)
        };
    
    public static readonly Expression<Func<PurchaseContentLogistic, PurchaseContentLogisticDto>> ToContentLogisticDto =
        x => new PurchaseContentLogisticDto
        {
            WeightKg = x.WeightKg,
            AreaM3 = x.AreaM3,
            Price = x.Price
        };
}