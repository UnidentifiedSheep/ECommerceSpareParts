using Cache;
using FluentAssertions;
using Main.Application;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions.Products;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class GetProductOrSetAsyncTests : IntegrationTest
{
    public GetProductOrSetAsyncTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task GetProductOrSetAsync_WhenProductExists_ReturnsProductFromDb()
    {
        var product = TestContext.Products[0];
        var producer = TestContext.ProducerTestContext.Producers
            .First(x => x.Id == product.ProducerId);
        var repository = GetRepository();

        await RemoveCachedProduct(product.Id);

        var result = await repository.GetProductOrSetAsync(product.Id);

        result.Id.Should().Be(product.Id);
        result.Sku.Should().Be(product.Sku.Value);
        result.Name.Should().Be(product.Name.Value);
        result.Description.Should().Be(product.Description);
        result.ProducerId.Should().Be(product.ProducerId);
        result.ProducerName.Should().Be(producer.Name);
        result.Stock.Should().Be(product.Stock.Value);
        result.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductOrSetAsync_WhenProductAlreadyCached_ReturnsCachedProduct()
    {
        var product = TestContext.Products[0];
        var repository = GetRepository();

        await RemoveCachedProduct(product.Id);

        var cached = await repository.GetProductOrSetAsync(product.Id);

        product.SetName("Updated product name");
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await repository.GetProductOrSetAsync(product.Id);

        result.Should().BeEquivalentTo(cached);
        result.Name.Should().NotBe("Updated product name");
    }

    [Fact]
    public async Task GetProductOrSetAsync_WhenProductDoesNotExist_ThrowsProductNotFoundException()
    {
        const int productId = int.MaxValue;
        var repository = GetRepository();

        await RemoveCachedProduct(productId);

        var act = () => repository.GetProductOrSetAsync(productId);

        await act.Should().ThrowAsync<ProductNotFoundException>();
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