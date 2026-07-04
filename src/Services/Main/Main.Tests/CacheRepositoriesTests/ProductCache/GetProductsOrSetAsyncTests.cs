using Cache;
using FluentAssertions;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Cache;
using Main.Application.Static;
using Main.Entities.Product;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class GetProductsOrSetAsyncTests : IntegrationTest
{
    public GetProductsOrSetAsyncTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task GetProductsOrSetAsync_WhenProductsExist_ReturnsProductsFromDb()
    {
        var products = TestContext.Products.Take(3).ToList();
        var repository = GetRepository();

        await RemoveCachedProducts(products.Select(x => x.Id));

        var result = await repository.GetProductsOrSetAsync(products.Select(x => x.Id));

        result.Should().HaveCount(products.Count);

        foreach (var product in products)
        {
            result.Should().ContainKey(product.Id);
            AssertProduct(result[product.Id], product);
        }
    }

    [Fact]
    public async Task GetProductsOrSetAsync_WhenSomeProductsAlreadyCached_ReturnsCachedAndDbProducts()
    {
        var cachedProduct = TestContext.Products[0];
        var dbProduct = TestContext.Products[1];
        var repository = GetRepository();

        await RemoveCachedProducts([cachedProduct.Id, dbProduct.Id]);

        var cachedBeforeUpdate =
            (await repository.GetProductsOrSetAsync([cachedProduct.Id]))[cachedProduct.Id];

        cachedProduct.SetName("Updated product name");
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var result = await repository.GetProductsOrSetAsync([cachedProduct.Id, dbProduct.Id]);

        result.Should().HaveCount(2);
        result[cachedProduct.Id].Should().BeEquivalentTo(cachedBeforeUpdate);
        result[cachedProduct.Id].Name.Should().NotBe("Updated product name");
        AssertProduct(result[dbProduct.Id], dbProduct);

        var rawCached = await repository.GetProductsAsync([dbProduct.Id]);
        rawCached.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(result[dbProduct.Id]);
    }

    [Fact]
    public async Task GetProductsOrSetAsync_WhenProductsDoNotExist_ReturnsEmptyDictionary()
    {
        var repository = GetRepository();
        var ids = new[] { int.MaxValue, int.MaxValue - 1 };

        await RemoveCachedProducts(ids);

        var result = await repository.GetProductsOrSetAsync(ids);

        result.Should().BeEmpty();
    }

    private IProductCacheRepository GetRepository()
    {
        return Scope.ServiceProvider.GetRequiredService<IProductCacheRepository>();
    }

    private Task RemoveCachedProducts(IEnumerable<int> productIds)
    {
        return Task.WhenAll(productIds.Select(RemoveCachedProduct));
    }

    private async Task RemoveCachedProduct(int productId)
    {
        await Scope.ServiceProvider
            .GetRequiredService<ICache>()
            .RemoveKeyAsync(CacheKeys.ProductCache.Product(productId));
    }

    private void AssertProduct(ProductDto dto, Product product)
    {
        var producer = TestContext.ProducerTestContext.Producers
            .First(x => x.Id == product.ProducerId);

        dto.Id.Should().Be(product.Id);
        dto.Sku.Should().Be(product.Sku.Value);
        dto.Name.Should().Be(product.Name.Value);
        dto.Description.Should().Be(product.Description);
        dto.ProducerId.Should().Be(product.ProducerId);
        dto.ProducerName.Should().Be(producer.Name);
        dto.Stock.Should().Be(product.Stock.Value);
        dto.Images.Should().BeEmpty();
    }
}