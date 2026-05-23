using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Cache;
using Cache.Extensions;
using Main.Application;
using Main.Application.Dtos.Product;
using Main.Application.Extensions.QueryExtensions;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Cache;

public class ProductCacheRepository(
    ICache rawCache,
    IReadRepository<ProductCross, (int, int)> crossesReadRepository,
    IReadRepository<Product, int> productReadRepository) : IProductCacheRepository
{
    public async Task<ProductDto> GetProductOrSetAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.ProductCache.Product(productId);
        var cached = await rawCache.GetAsync<ProductDto>(key);

        if (cached != null)
            return cached;

        var product = await GetProductFromDb(productId, cancellationToken);

        await rawCache.SetAsync(
            [(key, product)],
            CacheKeys.ProductCache.Ttl);

        return product;
    }

    public async Task<ProductDto?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await rawCache.GetAsync<ProductDto>(CacheKeys.ProductCache.Product(productId));
    }

    public async Task<Dictionary<int, ProductDto>> GetProductsOrSetAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        return await rawCache.GetOrSetManyAsync(
            ids,
            CacheKeys.ProductCache.Product,
            product => product.Id,
            missingIds => productReadRepository
                .Query
                .DictionaryProductDto(x => missingIds.Contains(x.Id), cancellationToken),
            CacheKeys.ProductCache.Ttl);
    }

    public async Task<IEnumerable<int>> GetProductCrossesAsync(
        int productId,
        string? sortBy,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.ProductCache.ProductCrosses(productId, sortBy);

        var cached = await rawCache.GetJsonArrayOrEmptyAsync<int>(key);

        if (cached.IsHit)
            return cached.Values;

        var crosses = await GetCrossesFromDb(productId, sortBy, cancellationToken);

        await rawCache.SetJsonArrayAsync(key, crosses, CacheKeys.ProductCache.Ttl);

        await rawCache.AddRelationsAsync(
            crosses.Prepend(productId),
            CacheKeys.ProductCache.ProductCrossRelations,
            key,
            CacheKeys.ProductCache.Ttl);

        return crosses;
    }

    public async Task InvalidateCrossesAsync(int productId)
    {
        var relationKey = CacheKeys.ProductCache.ProductCrossRelations(productId);
        await rawCache.InvalidateByRelationsAsync(relationKey);
    }

    public async Task<IReadOnlyList<ProductDto?>> GetProductsAsync(IEnumerable<int> ids)
    {
        return (await rawCache.GetAsync<ProductDto>(ids.Select(CacheKeys.ProductCache.Product)))
            .DeserializeMany<ProductDto>();
    }

    public Task InvalidateProductAsync(int productId)
    {
        return rawCache.RemoveKeyAsync(CacheKeys.ProductCache.Product(productId));
    }

    public Task InvalidateProductsAsync(IEnumerable<int> productIds)
    {
        return rawCache.RemoveKeysAsync(productIds.Select(CacheKeys.ProductCache.Product));
    }

    private Task<List<int>> GetCrossesFromDb(
        int productId,
        string? sortBy,
        CancellationToken cancellationToken = default)
    {
        return crossesReadRepository.Query
            .GetCrosses(productId)
            .SortBy(sortBy)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<ProductDto> GetProductFromDb(int id, CancellationToken cancellationToken)
    {
        return await productReadRepository
                   .Query
                   .FirstProductDtoAsync(x => x.Id == id, cancellationToken)
               ?? throw new ProductNotFoundException(id);
    }
}