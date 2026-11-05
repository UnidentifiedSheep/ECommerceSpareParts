using Application.Common.Interfaces.Cache;
using FluentAssertions;
using Main.Application.Interfaces.Cache;
using Main.Application.Products.Interfaces;
using Main.Application.Static;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
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
		var cacheInvalidator = GetCacheInvalidator();
		const int productId = int.MaxValue;

		await RemoveRelation(productId);

		var act = () => cacheInvalidator.InvalidateCrossesAsync(productId);

		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task InvalidateCrossesAsync_WhenRelationsExist_RemovesCrossesCacheAndRelation()
	{
		var productId = TestContext.ProductCrosses[0].LeftProductId;
		var productProvider = GetProductProvider();
		var cacheInvalidator = GetCacheInvalidator();

		await RemoveCrossesCache(productId);
		await productProvider.GetProductCrossesAsync(
			productId,
			CancellationToken);

		await cacheInvalidator.InvalidateCrossesAsync(productId);

		var cacheExists = await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId));
		cacheExists.Should().BeFalse();

		var relations = await GetRelations(productId);
		relations.Should().BeEmpty();
	}

	[Fact]
	public async Task InvalidateCrossesAsync_WhenCalledForCrossProduct_RemovesOriginalProductCrossesCache()
	{
		var productId = TestContext.ProductCrosses[0].LeftProductId;
		var productProvider = GetProductProvider();
		var cacheInvalidator = GetCacheInvalidator();

		await RemoveCrossesCache(productId);

		var crosses = (await productProvider.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();
		var crossProductId = crosses[0];

		await cacheInvalidator.InvalidateCrossesAsync(crossProductId);

		var cacheExists = await CacheKeyExists(CacheKeys.ProductCache.ProductCrosses(productId));
		cacheExists.Should().BeFalse();

		var relations = await GetRelations(crossProductId);
		relations.Should().BeEmpty();
	}

	private IProductProvider GetProductProvider() =>
		Scope.ServiceProvider.GetRequiredService<IProductProvider>();

	private IProductCacheInvalidator GetCacheInvalidator() =>
		Scope.ServiceProvider.GetRequiredService<IProductCacheInvalidator>();

	private async Task RemoveCrossesCache(int productId)
	{
		await Scope
			.ServiceProvider
			.GetRequiredService<ICache>()
			.RemoveKeyAsync(CacheKeys.ProductCache.ProductCrosses(productId));

		await RemoveRelation(productId);
	}

	private async Task RemoveRelation(int productId)
	{
		await Scope
			.ServiceProvider
			.GetRequiredService<ICache>()
			.RemoveKeyAsync(CacheKeys.ProductCache.ProductCrossRelations(productId));
	}

	private async Task<bool> CacheKeyExists(string key) =>
		await Scope.ServiceProvider.GetRequiredService<ICache>().KeyExistsAsync(key);

	private async Task<string[]> GetRelations(int productId)
	{
		return await Scope
			.ServiceProvider
			.GetRequiredService<ICache>()
			.GetFromSetAsync(CacheKeys.ProductCache.ProductCrossRelations(productId));
	}
}
