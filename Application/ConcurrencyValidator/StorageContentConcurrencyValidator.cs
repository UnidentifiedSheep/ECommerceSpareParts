using Core.Entities;
using Core.Interfaces;
using Core.StaticFunctions;

namespace Application.ConcurrencyValidator;

public class StorageContentConcurrencyValidator : IConcurrencyValidator<StorageContent>
{
    public bool IsValid(StorageContent item, string concurrencyCode, out string validCode)
    {
        validCode = ConcurrencyStatic.GetConcurrencyCode(item.Id, item.ArticleId,
            item.BuyPrice, item.CurrencyId, item.StorageName, item.BuyPriceInUsd, item.Count, item.PurchaseDatetime);
        return validCode == concurrencyCode;
    }
}