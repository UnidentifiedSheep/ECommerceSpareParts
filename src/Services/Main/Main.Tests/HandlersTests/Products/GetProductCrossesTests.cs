using Application.Common.Models;
using Application.Common.Querying;
using FluentAssertions;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Interfaces.Products;
using Main.Entities.Storage;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Products;

public sealed class GetProductCrossesTests : IntegrationTest
{
	public GetProductCrossesTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductCrossTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task CountSort_UsesTotalAcrossWarehousesAndPaginatesAfterSorting()
	{
		var (productId, ids) = await LoadCrosses();
		var storages = GetContext<StorageTestContext>().Storages.Take(2).ToArray();
		Context.ProductStocks.AddRange(
			ProductStock.Create(storages[0].Code, ids[0], 2),
			ProductStock.Create(storages[1].Code, ids[0], 10),
			ProductStock.Create(storages[0].Code, ids[1], 8));
		await Context.SaveChangesAsync(CancellationToken);

		var first = new GetProductCrossesQuery(productId, new Pagination(0, 1), ["count_desc"]);
		var second = new GetProductCrossesQuery(productId, new Pagination(1, 1), ["count_desc"]);
		var firstResult = await Mediator.Send(first, CancellationToken);
		var secondResult = await Mediator.Send(second, CancellationToken);

		firstResult.Crosses.Should().ContainSingle().Which.Id.Should().Be(ids[0]);
		secondResult.Crosses.Should().ContainSingle().Which.Id.Should().Be(ids[1]);
	}

	[Fact]
	public async Task CountSort_ReflectsStockChangesWhileCrossIdsAreCached()
	{
		var (productId, ids) = await LoadCrosses();
		var storage = GetContext<StorageTestContext>().Storages.First();
		var stock = ProductStock.Create(storage.Code, ids[0], 10);
		Context.ProductStocks.AddRange(stock, ProductStock.Create(storage.Code, ids[1], 5));
		await Context.SaveChangesAsync(CancellationToken);
		var item = new GetProductCrossesQuery(productId, new Pagination(0, 1), ["count_desc"]);

		var initial = await Mediator.Send(item, CancellationToken);
		initial.Crosses.Should().ContainSingle().Which.Id.Should().Be(ids[0]);

		stock.SetQuantity(0);
		await Context.SaveChangesAsync(CancellationToken);

		var updated = await Mediator.Send(item, CancellationToken);
		updated.Crosses.Should().ContainSingle().Which.Id.Should().Be(ids[1]);
	}

	[Fact]
	public async Task CountSort_ProductsWithoutStocksAreZeroAndTiesUseId()
	{
		var (productId, ids) = await LoadCrosses();
		var storage = GetContext<StorageTestContext>().Storages.First();
		Context.ProductStocks.Add(ProductStock.Create(storage.Code, ids[^1], 5));
		await Context.SaveChangesAsync(CancellationToken);
		var item = new GetProductCrossesQuery(productId, new Pagination(0, 100), ["count"]);

		var result = await Mediator.Send(item, CancellationToken);

		result.Crosses.Select(product => product.Id).Should().Equal(ids);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task CountSort_WithStorageArgument_UsesOnlySelectedWarehouse(bool parseFromText)
	{
		var (productId, ids) = await LoadCrosses();
		var storages = GetContext<StorageTestContext>().Storages.Take(2).ToArray();
		Context.ProductStocks.AddRange(
			ProductStock.Create(storages[0].Code, ids[0], 1),
			ProductStock.Create(storages[1].Code, ids[0], 20),
			ProductStock.Create(storages[0].Code, ids[1], 5),
			ProductStock.Create(storages[1].Code, ids[1], 2));
		await Context.SaveChangesAsync(CancellationToken);
		var sort = parseFromText
			? SortExpression.Parse($"count_{storages[0].Code}_desc")
			: new SortExpression("count", true, storages[0].Code);

		var result = await Mediator.Send(new GetProductCrossesQuery(
			productId, new Pagination(0, 2), [sort]), CancellationToken);

		result.Crosses.Select(product => product.Id).Should().Equal(ids[1], ids[0]);
	}

	[Theory]
	[InlineData("  CrOsSfIlTeR  ")]
	[InlineData("Ab 987654")]
	[InlineData("%")]
	[InlineData("_")]
	[InlineData("\\")]
	public async Task SearchTerm_FiltersCachedCrossesByNameOrNormalizedSku(string searchTerm)
	{
		var (productId, ids) = await LoadCrosses();
		var products = GetContext<ProductCrossTestContext>().ProductTestContext.Products;
		var target = products.Single(product => product.Id == ids[0]);
		target.SetName("CrossFilter % _ \\ literal");
		target.SetSku("AB-987654");
		var unrelated = products.First(product => product.Id != productId && !ids.Contains(product.Id));
		unrelated.SetName("CrossFilter % _ \\ literal");
		unrelated.SetSku("AB-987654-other");
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetProductCrossesQuery(
			productId, new Pagination(0, 100), SearchTerm: searchTerm), CancellationToken);

		result.Crosses.Should().ContainSingle().Which.Id.Should().Be(target.Id);
	}

	[Fact]
	public async Task SearchTerm_FiltersByProducerName()
	{
		var (productId, ids) = await LoadCrosses();
		var products = GetContext<ProductCrossTestContext>().ProductTestContext.Products;
		var target = products.Single(product => product.Id == ids[0]);
		var producer = GetContext<ProductCrossTestContext>().ProductTestContext.ProducerTestContext.Producers
			.Single(item => item.Id == target.ProducerId);
		var expected = products.Where(product => ids.Contains(product.Id) && product.ProducerId == producer.Id)
			.Select(product => product.Id).Order().ToArray();

		var result = await Mediator.Send(new GetProductCrossesQuery(
			productId, new Pagination(0, 100), SearchTerm: producer.Name), CancellationToken);

		result.Crosses.Select(product => product.Id).Should().Equal(expected);
	}

	[Theory]
	[InlineData("   ", true)]
	[InlineData("no-matching-cross-732198", false)]
	public async Task SearchTerm_EmptyLoadsAllAndUnmatchedReturnsEmpty(string searchTerm, bool expectAll)
	{
		var (productId, ids) = await LoadCrosses();
		var result = await Mediator.Send(new GetProductCrossesQuery(
			productId, new Pagination(0, 100), SearchTerm: searchTerm), CancellationToken);

		result.Crosses.Select(product => product.Id).Should().Equal(expectAll ? ids : []);
	}

	private async Task<(int ProductId, int[] CrossIds)> LoadCrosses()
	{
		var context = GetContext<ProductCrossTestContext>();
		var productId = context.ProductCrosses
			.SelectMany(cross => new[] { cross.LeftProductId, cross.RightProductId })
			.GroupBy(id => id).OrderByDescending(group => group.Count()).First().Key;
		var provider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		var ids = await provider.GetProductCrossesAsync(productId, CancellationToken);
		return (productId, ids.Order().ToArray());
	}
}
