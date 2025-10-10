using Core.Interfaces;
using Core.StaticFunctions;
using Main.Core.Entities;

namespace Main.Application.ConcurrencyValidator;

public class StorageContentConcurrencyValidator : IConcurrencyValidator<StorageContent>
{
    public bool IsValid(StorageContent item, string concurrencyCode, out string validCode)
    {
        validCode = ConcurrencyStatic.GetConcurrencyCode(item.Id, item.ArticleId,
            item.BuyPrice, item.CurrencyId, item.StorageName, item.BuyPriceInUsd, item.Count, item.PurchaseDatetime);
        return validCode == concurrencyCode;
    }
}