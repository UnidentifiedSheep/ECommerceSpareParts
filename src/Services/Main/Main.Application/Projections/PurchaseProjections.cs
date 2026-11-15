using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Entities.Purchase;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class PurchaseDtoProjectionProvider : ProjectionProviderBase<Purchase, PurchaseDto>
{
	public override Expression<Func<Purchase, PurchaseDto>> Projection { get; } =
		x => new PurchaseDto
		{
			Id = x.Id,
			Comment = x.Comment,
			CurrencyId = x.CurrencyId,
			PurchaseDatetime = x.PurchaseDatetime,
			StorageCode = x.Storage,
			SupplierId = x.SupplierUserId,
			SupplierOrganizationId = x.SupplierOrganizationId,
			TotalSum = x.Transaction.Amount,
			TransactionId = x.TransactionId
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	PurchaseContentDtoProjectionProvider : ProjectionProviderBase<PurchaseContent, PurchaseContentDto>
{
	public override Expression<Func<PurchaseContent, PurchaseContentDto>> Projection { get; } =
		x => new PurchaseContentDto
		{
			Id = x.Id,
			Count = x.Count,
			Comment = x.Comment,
			Price = x.Price,
			TotalSum = x.TotalSum,
			ProductId = x.ProductId
		};
}
