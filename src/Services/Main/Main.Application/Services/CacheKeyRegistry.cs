using Abstractions.Models;
using Application.Common.Abstractions;
using Main.Application.Handlers.Currencies.GetCurrencies;
using Main.Application.Handlers.Currencies.GetCurrencyById;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using Main.Application.Handlers.ProductWeight.GetProductWeight;

namespace Main.Application.Services;

public sealed class CacheKeyRegistry : CacheKeyRegistryBase
{
    public CacheKeyRegistry()
    {
        RegisterKey<GetCurrenciesResult, Pagination>(p => $"currencies:{p.Page}-{p.Size}");
        RegisterKey<GetCurrencyByIdResult, int>(x => $"currency:{x}");
        
        
        RegisterKey<GetProductCrossesResult, (int id, Pagination pagination, string? sortBy)>(
            x =>$"product-crosses:{x.id}-{x.pagination.Page}-{x.pagination.Size}-{x.sortBy}");
        RegisterKey<GetProductSizesResult, int>(x => $"product-size:{x}");
        RegisterKey<GetProductWeightResult, int>(x => $"product-weight:{x}");
    }
}