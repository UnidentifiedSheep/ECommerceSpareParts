using Application.Common.Interfaces.Cache;
using FluentAssertions;
using Main.Application.Products.Interfaces;
using Main.Application.Static;
using Main.Entities.Product;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.CacheRepositoriesTests.ProductCache;

public class GetProductCrossesAsyncTests : IntegrationTest
{
	public GetProductCrossesAsyncTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductCrossTestContext>();
	}

	private ProductCrossTestContext TestContext => GetContext<ProductCrossTestContext>();

	private IReadOnlyList<Product> Products => TestContext.ProductTestContext.Products;

	[Fact]
	public async Task GetProductCrossesAsync_WhenProductHasCrosses_ReturnsCrossesFromDb()
	{
		var productId = TestContext.ProductCrosses[0].LeftProductId;
		var repository = GetRepository();

		await RemoveCachedCrosses(productId);

		var result = (await repository.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();
		var expected = GetExpectedCrossIds(productId).OrderBy(x => x).ToList();

		result.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public async Task GetProductCrossesAsync_WhenProductHasOnlyRightSideCrosses_ReturnsLeftProducts()
	{
		var cross = TestContext.ProductCrosses[0];
		var productId = cross.RightProductId;
		var repository = GetRepository();

		await RemoveCachedCrosses(productId);

		var result = (await repository.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();

		result.Should().Contain(cross.LeftProductId);
		result.Should().BeEquivalentTo(GetExpectedCrossIds(productId));
	}

	[Fact]
	public async Task GetProductCrossesAsync_WhenProductHasNoCrosses_ReturnsEmptyCollection()
	{
		var productId = Products[5].Id;
		var repository = GetRepository();

		await RemoveCachedCrosses(productId);

		var result = await repository.GetProductCrossesAsync(
			productId,
			CancellationToken);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetProductCrossesAsync_WhenProductDoesNotExist_ReturnsEmptyCollection()
	{
		const int productId = int.MaxValue;
		var repository = GetRepository();

		await RemoveCachedCrosses(productId);

		var result = await repository.GetProductCrossesAsync(
			productId,
			CancellationToken);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetProductCrossesAsync_WhenCrossesAlreadyCached_ReturnsCachedCrosses()
	{
		var productId = TestContext.ProductCrosses[0].LeftProductId;
		var isolatedProductId = Products[5].Id;
		var repository = GetRepository();

		await RemoveCachedCrosses(productId);

		var cached = (await repository.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();

		await Context.ProductCrosses.AddAsync(
			ProductCross.Create(productId, isolatedProductId),
			CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ChangeTracker.Clear();

		var result = (await repository.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();

		result.Should().BeEquivalentTo(cached);
		result.Should().NotContain(isolatedProductId);
	}

	[Fact]
	public async Task GetProductsCrossesAsync_DuplicateIds_ReturnsRelationsForEachDistinctProduct()
	{
		var ids = TestContext.ProductCrosses.SelectMany(cross => new[] { cross.LeftProductId, cross.RightProductId })
			.Distinct().Take(2).ToArray();
		var result = await GetRepository().GetProductsCrossesAsync([ids[0], ids[1], ids[0]], CancellationToken);

		result.Should().HaveCount(2);
		foreach (var id in ids)
			result[id].Should().BeEquivalentTo(GetExpectedCrossIds(id));
	}

	[Fact]
	public async Task GetProductCrossesAsync_WhenCrossesLoaded_AddsRelationsForProductAndCrosses()
	{
		var productId = TestContext.ProductCrosses[0].LeftProductId;
		var repository = GetRepository();
		var cacheKey = CacheKeys.ProductCache.ProductCrosses(productId);

		await RemoveCachedCrosses(productId);

		var result = (await repository.GetProductCrossesAsync(
			productId,
			CancellationToken)).ToList();

		var productRelations = await GetRelations(productId);
		productRelations.Should().Contain(cacheKey);
		var productRelationTtl = await GetTtl(CacheKeys.ProductCache.ProductCrossRelations(productId));
		productRelationTtl.Should().NotBeNull();
		productRelationTtl.Value.Should().BePositive();
		productRelationTtl.Value.Should().BeLessThanOrEqualTo(CacheKeys.ProductCache.Ttl);

		foreach (var crossId in result)
		{
			var crossRelations = await GetRelations(crossId);
			crossRelations.Should().Contain(cacheKey);
			var crossRelationTtl = await GetTtl(CacheKeys.ProductCache.ProductCrossRelations(crossId));
			crossRelationTtl.Should().NotBeNull();
			crossRelationTtl.Value.Should().BePositive();
			crossRelationTtl.Value.Should().BeLessThanOrEqualTo(CacheKeys.ProductCache.Ttl);
		}
	}

	private IProductProvider GetRepository() => Scope.ServiceProvider.GetRequiredService<IProductProvider>();

	private IReadOnlyList<int> GetExpectedCrossIds(int productId)
	{
		return TestContext
			.ProductCrosses
			.Where(x => x.LeftProductId == productId || x.RightProductId == productId)
			.Select(x => x.LeftProductId == productId ? x.RightProductId : x.LeftProductId)
			.OrderBy(x => x)
			.ToList();
	}

	private async Task RemoveCachedCrosses(int productId)
	{
		var cache = Scope.ServiceProvider.GetRequiredService<ICache>();
		var cacheKey = CacheKeys.ProductCache.ProductCrosses(productId);
		var relationKey = CacheKeys.ProductCache.ProductCrossRelations(productId);

		await cache.RemoveKeyAsync(cacheKey);
		await cache.RemoveKeyAsync(relationKey);
	}

	private async Task<string[]> GetRelations(int productId)
	{
		return await Scope
			.ServiceProvider
			.GetRequiredService<ICache>()
			.GetFromSetAsync(CacheKeys.ProductCache.ProductCrossRelations(productId));
	}

	private async Task<TimeSpan?> GetTtl(string key)
	{
		var multiplexer = Scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
		return await multiplexer.GetDatabase().KeyTimeToLiveAsync($"test:{key}");
	}
}
