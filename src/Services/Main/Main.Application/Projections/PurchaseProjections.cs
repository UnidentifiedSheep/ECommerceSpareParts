using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Organizations;
using Main.Application.Dtos.Product;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Users;
using Main.Entities.Currency;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.User;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseDtoProjectionProvider : ProjectionProviderBase<Purchase, PurchaseDto>
{
	public PurchaseDtoProjectionProvider(
		IProjectionProvider<Currency, CurrencyDto> currencyProjection,
		IProjectionProvider<User, UserDto> userProjection,
		IProjectionProvider<Organization, OrganizationDto> organizationProjection)
	{
		var currencyToDto = currencyProjection.Projection;
		var userToDto = userProjection.Projection;
		var organizationToDto = organizationProjection.Projection;

		Projection = x => new PurchaseDto
		{
			Id = x.Id,
			Comment = x.Comment,
			Currency = currencyToDto.Invoke(x.Currency),
			PurchaseDatetime = x.PurchaseDatetime,
			StorageCode = x.Storage,
			Supplier = userToDto.Invoke(x.SupplierUser),
			SupplierOrganization = organizationToDto.Invoke(x.SupplierOrganization),
			TotalSum = x.Transaction.Amount,
			TransactionId = x.TransactionId
		};
	}

	public override Expression<Func<Purchase, PurchaseDto>> Projection { get; }
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	PurchaseContentDtoProjectionProvider : ProjectionProviderBase<PurchaseContent, PurchaseContentDto>
{
	public PurchaseContentDtoProjectionProvider(
		IProjectionProvider<Product, ProductDto> productProjection)
	{
		var productToDto = productProjection.Projection;

		Projection = x => new PurchaseContentDto
		{
			Id = x.Id,
			Count = x.Count,
			Comment = x.Comment,
			Price = x.Price,
			TotalSum = x.TotalSum,
			Product = productToDto.Invoke(x.Product)
		};
	}

	public override Expression<Func<PurchaseContent, PurchaseContentDto>> Projection { get; }
}
