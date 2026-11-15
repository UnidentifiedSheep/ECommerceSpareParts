using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class SaleDtoProjectionProvider : ProjectionProviderBase<Sale, SaleDto>
{
	public override Expression<Func<Sale, SaleDto>> Projection { get; } =
		x => new SaleDto
		{
			Id = x.Id,
			BuyerId = x.UserId,
			OrganizationId = x.OrganizationId,
			Comment = x.Comment,
			CurrencyId = x.CurrencyId,
			SaleDatetime = x.SaleDatetime,
			StorageCode = x.StorageCode,
			TotalSum = x.Transaction.Amount,
			TransactionId = x.TransactionId,
			RowVersion = x.RowVersion,
			State = x.State
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class SaleContentDtoProjectionProvider : ProjectionProviderBase<SaleContent, SaleContentDto>
{
	public override Expression<Func<SaleContent, SaleContentDto>> Projection { get; } =
		x => new SaleContentDto
		{
			Id = x.Id,
			Count = x.Count,
			DiscountedPrice = x.DiscountedPrice,
			TotalSum = x.TotalSum,
			OriginalPrice = x.OriginalPrice,
			Comment = x.Comment,
			ProductId = x.ProductId
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	SaleContentDetailDtoProjectionProvider : ProjectionProviderBase<SaleContentDetail, SaleContentDetailDto>
{
	public override Expression<Func<SaleContentDetail, SaleContentDetailDto>> Projection { get; } =
		x => new SaleContentDetailDto
		{
			Id = x.Id,
			BuyPrice = x.BuyPrice,
			Count = x.Count,
			CurrencyId = x.CurrencyId,
			PurchaseDatetime = x.PurchaseDatetime,
			SaleContentId = x.SaleContentId,
			StorageContentId = x.StorageContentId
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	ProductSaleHistoryDtoProjectionProvider : ProjectionProviderBase<SaleContent, ProductSaleHistoryDto>
{
	public override Expression<Func<SaleContent, ProductSaleHistoryDto>> Projection { get; } = x =>
		new ProductSaleHistoryDto
		{
			SaleContentId = x.Id,
			OrganizationId = x.Sale.OrganizationId,
			CurrencyId = x.Sale.CurrencyId,
			ProductId = x.ProductId,
			StorageCode = x.Sale.StorageCode,
			Quantity = x.Count,
			OriginalPrice = x.OriginalPrice,
			DiscountedPrice = x.DiscountedPrice,
			AverageBuyPrice = x.Details.Sum(detail => detail.BuyPrice * detail.Count) / x.Count,
			SaleDate = x.Sale.SaleDatetime,
			WhoCreated = x.Sale.WhoCreated
		};
}
