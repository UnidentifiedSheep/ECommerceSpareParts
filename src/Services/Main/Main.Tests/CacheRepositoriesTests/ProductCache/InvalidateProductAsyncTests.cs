using FluentAssertions;
using Main.Application.Interfaces.Cache;
using Main.Application.Interfaces.Products;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class InvalidateProductAsyncTests : IntegrationTest
{
    public InvalidateProductAsyncTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task InvalidateProductAsync_WhenProductCached_RemovesProductCache()
    {
        var product = TestContext.Products[0];
        var productProvider = GetProductProvider();
        var cacheInvalidator = GetCacheInvalidator();

        await productProvider.GetProductOrSetAsync(product.Id);

        await cacheInvalidator.InvalidateProductAsync(product.Id);

        var result = await productProvider.GetProductAsync(product.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateProductAsync_WhenProductCached_AllowsReloadingUpdatedProductFromDb()
    {
        var product = TestContext.Products[0];
        var productProvider = GetProductProvider();
        var cacheInvalidator = GetCacheInvalidator();

        await productProvider.GetProductOrSetAsync(product.Id);

        product.SetName("Updated product name");
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await cacheInvalidator.InvalidateProductAsync(product.Id);

        var result = await productProvider.GetProductOrSetAsync(product.Id);

        result.Name.Should().Be("Updated product name");
    }

    [Fact]
    public async Task InvalidateProductsAsync_WhenProductsCached_RemovesAllProductCaches()
    {
        var products = TestContext.Products.Take(3).ToList();
        var productProvider = GetProductProvider();
        var cacheInvalidator = GetCacheInvalidator();

        await productProvider.GetProductsOrSetAsync(products.Select(x => x.Id));

        await cacheInvalidator.InvalidateProductsAsync(products.Select(x => x.Id));

        var result = await productProvider.GetProductsAsync(products.Select(x => x.Id));

        result.Should().HaveCount(products.Count);
        result.Should().OnlyContain(x => x == null);
    }

    [Fact]
    public async Task InvalidateProductsAsync_WhenProductIdsAreEmpty_DoesNotThrow()
    {
        var cacheInvalidator = GetCacheInvalidator();

        var act = () => cacheInvalidator.InvalidateProductsAsync([]);

        await act.Should().NotThrowAsync();
    }

    private IProductProvider GetProductProvider()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductProvider>();
    }

    private IProductCacheInvalidator GetCacheInvalidator()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductCacheInvalidator>();
    }
}
