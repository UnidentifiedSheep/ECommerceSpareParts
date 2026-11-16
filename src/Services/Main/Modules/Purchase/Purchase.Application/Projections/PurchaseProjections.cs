using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Purchase.Purchase;
using Main.Modules.Purchase.Application.Dtos.Purchase;

namespace Main.Modules.Purchase.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	PurchaseDtoProjectionProvider : ProjectionProviderBase<Entities.Purchase.Purchase.Purchase, PurchaseDto>
{
	public override Expression<Func<Entities.Purchase.Purchase.Purchase, PurchaseDto>> Projection { get; } =
		x => new PurchaseDto
		{
			Id = x.Id,
			Comment = x.Comment,
			CurrencyId = x.CurrencyId,
			PurchaseDatetime = x.PurchaseDatetime,
			StorageCode = x.Storage,
			SupplierId = x.SupplierUserId,
			SupplierOrganizationId = x.SupplierOrganizationId,
			TotalSum = x.Contents.Sum(content => content.TotalSum),
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
