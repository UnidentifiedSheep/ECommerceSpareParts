using FluentAssertions;
using Main.Api.GraphQl.Types.Product;
using Main.Entities.Analytics;
using Main.Entities.Product.Product;
using Main.Modules.Analytics.Abstractions.Models.CoPurchase;
using Main.Modules.Analytics.Application.Models.CoPurchase;
using Main.Modules.Analytics.Application.Services;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Analytics;

public class ProductAlsoBoughtTests : IntegrationTest
{
	public ProductAlsoBoughtTests(CombinedContainerFixture fixture) : base(fixture)
		=> RegisterBasicContext<ProductTestContext>();

	private int ProductId(int index) => GetContext<ProductTestContext>().Products[index].Id;

	[Fact]
	public async Task AlsoBought_ReturnsRecommendationsInOrder()
	{
		await AddRunAsync(false, (ProductId(0), [ProductId(1), ProductId(2)]));

		var result = await AlsoBought(ProductId(0));

		result.Should().Equal(ProductId(1), ProductId(2));
	}

	[Fact]
	public async Task AlsoBought_UsesCrossWhenProductHasNoRecommendations()
	{
		await AddRunAsync(false, (ProductId(0), [ProductId(1)]));
		await Context.Set<ProductCross>().AddAsync(ProductCross.Create(ProductId(0), ProductId(3)), CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await AlsoBought(ProductId(3));

		result.Should().Equal(ProductId(1));
	}

	[Fact]
	public async Task AlsoBought_PrefersPinnedRunOverLatest()
	{
		await AddRunAsync(true, (ProductId(0), [ProductId(1)]));
		await AddRunAsync(false, (ProductId(0), [ProductId(2)]));

		var result = await AlsoBought(ProductId(0));

		result.Should().Equal(ProductId(1));
	}

	[Fact]
	public async Task AlsoBought_ReturnsEmptyWithoutCompletedRuns()
		=> (await AlsoBought(ProductId(0))).Should().BeEmpty();

	private async Task<IReadOnlyList<int>> AlsoBought(int productId)
		=> (await new GqlProduct(productId).GetAlsoBoughtAsync(Mediator, CancellationToken))
			.Select(product => product.Id)
			.ToList();

	private async Task AddRunAsync(bool pinned, params (int ProductId, int[] Related)[] products)
	{
		var run = CoPurchaseRunFactory.CreateRun(
			new CoPurchaseParameters
			{
				From = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
				To = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
			},
			null,
			null);
		if (pinned)
			run.Pin();
		run.Complete(null, products.Length);

		var items = products.Select(product => CoPurchaseRunFactory.CreateItem(
				run.Id,
				new CoPurchaseProductResult(
					product.ProductId,
					10,
					product.Related
						.Select((relatedId, index) => new CoPurchaseRelated(
							[new CoPurchaseRelatedMember(relatedId, 5 - index)],
							5 - index,
							0.5m - index * 0.1m,
							2m))
						.ToList())))
			.ToList();

		await Context.Set<AnalyticsRun>().AddAsync(run, CancellationToken);
		await Context.Set<AnalyticsRunItem>().AddRangeAsync(items, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
	}
}
