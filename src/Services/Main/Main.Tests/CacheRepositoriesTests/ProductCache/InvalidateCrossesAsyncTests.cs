using Cache;
using FluentAssertions;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class InvalidateCrossesAsyncTests : IntegrationTest
{
    public InvalidateCrossesAsyncTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductCrossTestContext>();
    }

    private ProductCrossTestContext TestContext => GetContext<ProductCrossTestContext>();

    [Fact]
    public async Task InvalidateCrossesAsync_WhenRelationsDoNotExist_DoesNotThrow()
    {
        var repository = GetRepository();
        const int productId = int.MaxValue;

        await RemoveRelation(productId);

        var act = () => repository.InvalidateCrossesAsync(productId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InvalidateCrossesAsync_WhenRelationsExist_RemovesCrossesCacheAndRelation()
    {
        var productId = TestContext.ProductCrosses[0].LeftProductId;
        var repository = GetRepository();

        await RemoveCrossesCache(productId, null);
        await repository.GetProductCrossesAsync(productId, null);

        await repository.InvalidateCrossesAsync(productId);

        var cacheExists = await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId, null));
        cacheExists.Should().BeFalse();

        var relations = await GetRelations(productId);
        relations.Should().BeEmpty();
    }

    [Fact]
    public async Task InvalidateCrossesAsync_WhenMultipleSortKeysExist_RemovesAllRelatedCrossesCaches()
    {
        var productId = GetProductIdWithAtLeastTwoCrosses();
        var repository = GetRepository();

        await RemoveCrossesCache(productId, null);
        await RemoveCrossesCache(productId, "id_desc");

        await repository.GetProductCrossesAsync(productId, null);
        await repository.GetProductCrossesAsync(productId, "id_desc");

        await repository.InvalidateCrossesAsync(productId);

        var defaultCacheExists = await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId, null));
        defaultCacheExists.Should().BeFalse();

        var sortedCacheExists =
            await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId, "id_desc"));
        sortedCacheExists.Should().BeFalse();

        var relations = await GetRelations(productId);
        relations.Should().BeEmpty();
    }

    [Fact]
    public async Task InvalidateCrossesAsync_WhenCalledForCrossProduct_RemovesOriginalProductCrossesCache()
    {
        var productId = TestContext.ProductCrosses[0].LeftProductId;
        var repository = GetRepository();

        await RemoveCrossesCache(productId, null);

        var crosses = (await repository.GetProductCrossesAsync(productId, null)).ToList();
        var crossProductId = crosses[0];

        await repository.InvalidateCrossesAsync(crossProductId);

        var cacheExists = await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId, null));
        cacheExists.Should().BeFalse();

        var relations = await GetRelations(crossProductId);
        relations.Should().BeEmpty();
    }

    private IProductCacheRepository GetRepository()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductCacheRepository>();
    }

    private int GetProductIdWithAtLeastTwoCrosses()
    {
        return TestContext.ProductCrosses
            .SelectMany(x => new[] { x.LeftProductId, x.RightProductId })
            .GroupBy(x => x)
            .Where(x => x.Count() >= 2)
            .OrderByDescending(x => x.Count())
            .Select(x => x.Key)
            .First();
    }

    private async Task RemoveCrossesCache(int productId, string? sortBy)
    {
        await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .RemoveKeyAsync(CacheKeys.ProductCache.ProductCrosses(productId, sortBy));

        await RemoveRelation(productId);
    }

    private async Task RemoveRelation(int productId)
    {
        await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .RemoveKeyAsync(CacheKeys.ProductCache.ProductCrossRelations(productId));
    }

    private async Task<bool> CacheKeyExists(string key)
    {
        return await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .KeyExistsAsync(key);
    }

    private async Task<string[]> GetRelations(int productId)
    {
        return await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .GetFromSetAsync(CacheKeys.ProductCache.ProductCrossRelations(productId));
    }
}