using FluentAssertions;
using Main.Application.Interfaces.Cache;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
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
        var repository = GetRepository();

        await repository.GetProductOrSetAsync(product.Id);

        await repository.InvalidateProductAsync(product.Id);

        var result = await repository.GetProductAsync(product.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateProductAsync_WhenProductCached_AllowsReloadingUpdatedProductFromDb()
    {
        var product = TestContext.Products[0];
        var repository = GetRepository();

        await repository.GetProductOrSetAsync(product.Id);

        product.SetName("Updated product name");
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await repository.InvalidateProductAsync(product.Id);

        var result = await repository.GetProductOrSetAsync(product.Id);

        result.Name.Should().Be("Updated product name");
    }

    [Fact]
    public async Task InvalidateProductsAsync_WhenProductsCached_RemovesAllProductCaches()
    {
        var products = TestContext.Products.Take(3).ToList();
        var repository = GetRepository();

        await repository.GetProductsOrSetAsync(products.Select(x => x.Id));

        await repository.InvalidateProductsAsync(products.Select(x => x.Id));

        var result = await repository.GetProductsAsync(products.Select(x => x.Id));

        result.Should().HaveCount(products.Count);
        result.Should().OnlyContain(x => x == null);
    }

    [Fact]
    public async Task InvalidateProductsAsync_WhenProductIdsAreEmpty_DoesNotThrow()
    {
        var repository = GetRepository();

        var act = () => repository.InvalidateProductsAsync([]);

        await act.Should().NotThrowAsync();
    }

    private IProductCacheRepository GetRepository()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductCacheRepository>();
    }
}
