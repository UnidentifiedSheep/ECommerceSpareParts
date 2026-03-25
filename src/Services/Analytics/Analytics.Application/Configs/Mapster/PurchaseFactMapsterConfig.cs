using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Entities;
using Mapster;

using ContractPurchase = Contracts.Models.Purchase.Purchase;
using ContractPurchaseContent = Contracts.Models.Purchase.PurchaseContent;

namespace Analytics.Application.Configs.Mapster;

public static class PurchaseFactMapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ContractPurchase, PurchaseFactUpsertDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.SupplierId, s => s.SupplierId)
            .Map(d => d.CreatedAt, s => s.CreationDatetime)
            .Map(d => d.LastUpdatedAt, s => s.LastUpdatedAt)
            .Map(
                d => d.Content, 
                s => s.PurchaseContents.Adapt<List<PurchaseFactUpsertDto>>());
        
        TypeAdapterConfig<ContractPurchaseContent, PurchaseContentUpsertDto>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count);
        
        
        TypeAdapterConfig<PurchaseFactUpsertDto, PurchasesFact>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.ProcessedAt, s => DateTime.UtcNow)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.SupplierId, s => s.SupplierId);
        
        TypeAdapterConfig<PurchaseContentUpsertDto, PurchaseContent>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count);
    }
}