using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Storage;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Projections;

public static class StorageContentProjections
{
    public static readonly Expression<Func<StorageContent, StorageContentDto>> ToStorageContentDto =
        x => new StorageContentDto
        {
            Id = x.Id,
            StorageName = x.StorageName,
            ProductId = x.ProductId,
            Count = x.Count,
            BuyPrice = x.BuyPrice,
            PurchaseDatetime = x.PurchaseDatetime,
            RowVersion = x.RowVersion,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency)
        };
}