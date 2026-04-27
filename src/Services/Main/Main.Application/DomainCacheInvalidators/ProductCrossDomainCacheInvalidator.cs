using Abstractions.Interfaces.Cache;
using Abstractions.Interfaces.RelatedData;
using Application.Common.Abstractions;
using Main.Entities.Product;

namespace Main.Application.DomainCacheInvalidators;

public sealed class ProductCrossDomainCacheInvalidator(
    IRelatedDataRepository<ProductCross> relatedDataRepository, 
    ICache cache) 
    : DomainCacheInvalidatorBase<ProductCross, int>(relatedDataRepository, cache)
{
    
}