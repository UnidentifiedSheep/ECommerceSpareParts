using System.Linq.Expressions;
using LinqKit;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Entities.Storage;

namespace Main.Application.Handlers.Currencies.Projections;

public static class StorageContentProjections
{
    public static Expression<Func<StorageContent, StorageContentDto>> ToStorageContentDto =
        x => new StorageContentDto
        {
            Id = x.Id,
            StorageName = x.StorageName,
            ProductId = x.ProductId,
            Count = x.Count,
            BuyPrice = x.BuyPrice,
            PurchaseDatetime =  x.PurchaseDatetime,
            Currency = CurrencyProjections.ToDto.Invoke(x.Currency)
        };
    
    public static readonly Func<StorageContent, StorageContentDto> ToStorageContentDtoFunc =
        ToStorageContentDto.Compile();
}