using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Dtos.Storage;

namespace Main.Modules.Storage.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	StorageContentDtoProjectionProvider : ProjectionProviderBase<StorageContent, StorageContentDto>
{
	public override Expression<Func<StorageContent, StorageContentDto>> Projection { get; } =
		x => new StorageContentDto
		{
			Id = x.Id,
			StorageCode = x.StorageCode,
			ProductId = x.ProductId,
			Count = x.Count,
			BuyPrice = x.BuyPrice,
			PurchaseDatetime = x.PurchaseDatetime,
			RowVersion = x.RowVersion,
			CurrencyId = x.CurrencyId
		};
}
