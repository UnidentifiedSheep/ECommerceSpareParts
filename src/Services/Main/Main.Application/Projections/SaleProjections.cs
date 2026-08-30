using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Sale;
using Main.Application.Dtos.Users;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Sale;
using Main.Entities.User;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class SaleDtoProjectionProvider : ProjectionProviderBase<Sale, SaleDto>
{
	public SaleDtoProjectionProvider(
		IProjectionProvider<User, UserDto> userProjection,
		IProjectionProvider<Organization, OrganizationDto> organizationProjection,
		IProjectionProvider<Currency, CurrencyDto> currencyProjection)
	{
		var userToDto = userProjection.Projection;
		var organizationToDto = organizationProjection.Projection;
		var currencyToDto = currencyProjection.Projection;

		Projection = x => new SaleDto
		{
			Id = x.Id,
			Buyer = userToDto.Invoke(x.User),
			Organization = organizationToDto.Invoke(x.Organization),
			Comment = x.Comment,
			Currency = currencyToDto.Invoke(x.Currency),
			SaleDatetime = x.SaleDatetime,
			StorageCode = x.StorageCode,
			TotalSum = x.Transaction.Amount,
			TransactionId = x.TransactionId,
			RowVersion = x.RowVersion,
			State = x.State
		};
	}

	public override Expression<Func<Sale, SaleDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class SaleContentDtoProjectionProvider : ProjectionProviderBase<SaleContent, SaleContentDto>
{
	public SaleContentDtoProjectionProvider(
		IProjectionProvider<Product, ProductDto> productProjection,
		IProjectionProvider<SaleContentDetail, SaleContentDetailDto> detailProjection)
	{
		var productToDto = productProjection.Projection;
		var detailToDto = detailProjection.Projection;

		Projection = x => new SaleContentDto
		{
			Id = x.Id,
			Count = x.Count,
			Price = x.Price,
			TotalSum = x.TotalSum,
			Discount = x.Discount,
			Comment = x.Comment,
			Product = productToDto.Invoke(x.Product),
			Details = x.Details.Select(z => detailToDto.Invoke(z)).ToList()
		};
	}

	public override Expression<Func<SaleContent, SaleContentDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	SaleContentDetailDtoProjectionProvider : ProjectionProviderBase<SaleContentDetail, SaleContentDetailDto>
{
	public SaleContentDetailDtoProjectionProvider(
		IProjectionProvider<Currency, CurrencyDto> currencyProjection)
	{
		var currencyToDto = currencyProjection.Projection;

		Projection = x => new SaleContentDetailDto
		{
			Id = x.Id,
			BuyPrice = x.BuyPrice,
			Count = x.Count,
			Currency = currencyToDto.Invoke(x.Currency),
			PurchaseDatetime = x.PurchaseDatetime,
			SaleContentId = x.SaleContentId,
			StorageContentId = x.StorageContentId
		};
	}

	public override Expression<Func<SaleContentDetail, SaleContentDetailDto>> Projection { get; }
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
			Discount = x.Discount,
			Price = x.Price,
			AverageBuyPrice = x.Details.Sum(detail => detail.BuyPrice * detail.Count) / x.Count,
			SaleDate = x.Sale.SaleDatetime,
			WhoCreated = x.Sale.WhoCreated
		};
}
