using Cache;
using FluentAssertions;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class GetProductAsyncTests : IntegrationTest
{
    public GetProductAsyncTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task GetProductAsync_WhenProductCached_ReturnsCachedProduct()
    {
        var product = TestContext.Products[0];
        var repository = GetRepository();

        await RemoveCachedProduct(product.Id);

        var cached = await repository.GetProductOrSetAsync(product.Id);

        product.SetName("Updated product name");
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await repository.GetProductAsync(product.Id);

        result.Should().BeEquivalentTo(cached);
        result!.Name.Should().NotBe("Updated product name");
    }

    [Fact]
    public async Task GetProductAsync_WhenProductNotCached_ReturnsNull()
    {
        var product = TestContext.Products[0];
        var repository = GetRepository();

        await RemoveCachedProduct(product.Id);

        var result = await repository.GetProductAsync(product.Id);

        result.Should().BeNull();
    }

    private IProductCacheRepository GetRepository()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductCacheRepository>();
    }

    private async Task RemoveCachedProduct(int productId)
    {
        await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .RemoveKeyAsync(CacheKeys.ProductCache.Product(productId));
    }
}