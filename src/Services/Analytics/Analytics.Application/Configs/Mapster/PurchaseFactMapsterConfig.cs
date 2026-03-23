using Analytics.Abstractions.Dtos.PurchaseFact;
using Analytics.Entities;
using Mapster;

namespace Analytics.Application.Configs.Mapster;

public static class PurchaseFactMapsterConfig
{
    public static void Configure()
    {

        TypeAdapterConfig<PurchaseFactUpsertDto, PurchasesFact>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.CreatedAt, s => s.CreatedAt)
            .Map(d => d.CurrencyId, s => s.CurrencyId)
            .Map(d => d.SupplierId, s => s.SupplierId)
            .Map(d => d.PurchaseContents, s => s.Content);
        
        TypeAdapterConfig<PurchaseContentUpsertDto, PurchaseContent>.NewConfig()
            .IgnoreNonMapped(true)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.ArticleId, s => s.ArticleId)
            .Map(d => d.Price, s => s.Price)
            .Map(d => d.Count, s => s.Count);
    }
}