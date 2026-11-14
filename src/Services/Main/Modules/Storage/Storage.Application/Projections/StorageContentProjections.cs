using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Currencies;
using Main.Application.Dtos.Storage;
using Main.Entities.Currency;
using Main.Entities.Storage;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	StorageContentDtoProjectionProvider : ProjectionProviderBase<StorageContent, StorageContentDto>
{
	public StorageContentDtoProjectionProvider(IProjectionProvider<Currency, CurrencyDto> currencyProjection)
	{
		var currencyToDto = currencyProjection.Projection;

		Projection = x => new StorageContentDto
		{
			Id = x.Id,
			StorageCode = x.StorageCode,
			ProductId = x.ProductId,
			Count = x.Count,
			BuyPrice = x.BuyPrice,
			PurchaseDatetime = x.PurchaseDatetime,
			RowVersion = x.RowVersion,
			Currency = currencyToDto.Invoke(x.Currency)
		};
	}

	public override Expression<Func<StorageContent, StorageContentDto>> Projection { get; }
}
