using Mapster;
using Purchase = Main.Entities.Purchase;
using PurchaseContent = Main.Entities.PurchaseContent;
using ContractPurchase = Contracts.Models.Purchase.Purchase;
using ContractPurchaseContent = Contracts.Models.Purchase.PurchaseContent;

namespace Main.Application.Configs.Mapster.ContractMappings;

public static class PurchaseContractConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Purchase, ContractPurchase>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.CreatedUserId, s => s.CreatedUserId)
            .Map(d => d.SupplierId, s => s.SupplierId)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.PurchaseDatetime, s => s.PurchaseDatetime)
            .Map(d => d.CreationDatetime, s => s.CreationDatetime)
            .Map(d => d.LastUpdatedAt, s => s.UpdateDatetime)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.TransactionId, s => s.TransactionId)
            .Map(d => d.Storage, s => s.Storage)
            .Map(d => d.State, s => s.State)
            .Map(d => d.PurchaseContents, s => s.PurchaseContents);

        TypeAdapterConfig<PurchaseContent, ContractPurchaseContent>.NewConfig()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.PurchaseId, s => s.PurchaseId)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Count, s => s.Count)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.TotalSum, s => s.TotalSum)
            .Map(d => d.Comment, s => s.Comment)
            .Map(d => d.StorageContentId, s => s.StorageContentId);
    }
}