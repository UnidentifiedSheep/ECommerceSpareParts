using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Sale;
using Tests.DataBuilders.Sale;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSaleContentDetailsByIdsTests : IntegrationTest
{
	public GetSaleContentDetailsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedDetailsGroupedByContentWithoutDuplicates()
	{
		var first = TestContext.Sale.Contents[0];
		var second = await AddContent();
		await AddContent();

		var result = await Mediator.Send(
			new GetSaleContentDetailsByIdsQuery([first.Id, second.Id, first.Id, int.MaxValue]), CancellationToken);

		result.Details.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Details[first.Id].Select(detail => detail.Id)
			.Should().Equal(first.Details.Select(detail => detail.Id).Order());
		result.Details[second.Id].Select(detail => detail.Id)
			.Should().Equal(second.Details.Select(detail => detail.Id).Order());
	}

	[Fact]
	public async Task SingleContent_ReturnsItsDetails()
	{
		var content = TestContext.Sale.Contents[0];

		var result = await Mediator.Send(new GetSaleContentDetailsByIdsQuery(content.Id), CancellationToken);

		result.Details.Keys.Should().Equal(content.Id);
		result.Details[content.Id].Select(detail => detail.Id)
			.Should().Equal(content.Details.Select(detail => detail.Id).Order());
	}

	[Fact]
	public async Task MissingContent_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSaleContentDetailsByIdsQuery(int.MaxValue), CancellationToken);

		result.Details.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSaleContentDetailsByIdsQuery(Array.Empty<int>()), CancellationToken);

		result.Details.Should().BeEmpty();
	}

	private async Task<SaleContent> AddContent()
	{
		var content = new SaleContentBuilder(Faker)
			.WithProductId(TestContext.Product.Id)
			.WithCurrencyId(TestContext.Sale.CurrencyId)
			.WithStorageContentIds([TestContext.StorageContent.Id])
			.WithCount(2).WithDetailsCount(2).Build();
		TestContext.Sale.AddContent(content);
		await Context.SaveChangesAsync(CancellationToken);
		return content;
	}
}
