using Application.Common.Models;
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

		var first = new GetProductCrossesItem(productId, new Pagination(0, 1), ["count_desc"]);
		var second = new GetProductCrossesItem(productId, new Pagination(1, 1), ["count_desc"]);
		var result = await Mediator.Send(new GetProductCrossesQuery([first, second]), CancellationToken);

		result.Crosses[first].Should().ContainSingle().Which.Id.Should().Be(ids[0]);
		result.Crosses[second].Should().ContainSingle().Which.Id.Should().Be(ids[1]);
	}

	[Fact]
	public async Task CountSort_ReflectsStockChangesWhileCrossIdsAreCached()
	{
		var (productId, ids) = await LoadCrosses();
		var storage = GetContext<StorageTestContext>().Storages.First();
		var stock = ProductStock.Create(storage.Code, ids[0], 10);
		Context.ProductStocks.AddRange(stock, ProductStock.Create(storage.Code, ids[1], 5));
		await Context.SaveChangesAsync(CancellationToken);
		var item = new GetProductCrossesItem(productId, new Pagination(0, 1), ["count_desc"]);

		var initial = await Mediator.Send(new GetProductCrossesQuery([item]), CancellationToken);
		initial.Crosses[item].Should().ContainSingle().Which.Id.Should().Be(ids[0]);

		stock.SetQuantity(0);
		await Context.SaveChangesAsync(CancellationToken);

		var updated = await Mediator.Send(new GetProductCrossesQuery([item]), CancellationToken);
		updated.Crosses[item].Should().ContainSingle().Which.Id.Should().Be(ids[1]);
	}

	[Fact]
	public async Task CountSort_ProductsWithoutStocksAreZeroAndTiesUseId()
	{
		var (productId, ids) = await LoadCrosses();
		var storage = GetContext<StorageTestContext>().Storages.First();
		Context.ProductStocks.Add(ProductStock.Create(storage.Code, ids[^1], 5));
		await Context.SaveChangesAsync(CancellationToken);
		var item = new GetProductCrossesItem(productId, new Pagination(0, 100), ["count"]);

		var result = await Mediator.Send(new GetProductCrossesQuery([item]), CancellationToken);

		result.Crosses[item].Select(product => product.Id).Should().Equal(ids);
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
