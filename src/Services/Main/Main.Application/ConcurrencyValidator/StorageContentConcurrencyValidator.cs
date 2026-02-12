using Abstractions.Interfaces;
using Main.Entities;
using Utils;

namespace Main.Application.ConcurrencyValidator;

public class StorageContentConcurrencyValidator : IConcurrencyValidator<StorageContent>
{
    public bool IsValid(StorageContent item, string concurrencyCode, out string validCode)
    {
        validCode = HashUtils.ComputeHash(item.Id, item.ArticleId,
            item.BuyPrice, item.CurrencyId, item.StorageName, item.BuyPriceInUsd, item.Count, item.PurchaseDatetime);
        return validCode == concurrencyCode;
    }
}