using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Sale;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesContentsByIdsTests : IntegrationTest
{
	public GetSalesContentsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedContentsGroupedBySaleWithoutDuplicates()
	{
		var first = TestContext.Sale;
		var second = await CreateSale(withContents: true);
		await CreateSale(withContents: true);

		var result = await Mediator.Send(
			new GetSalesContentsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Contents.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Contents[first.Id].Select(content => content.Id)
			.Should().Equal(first.Contents.Select(content => content.Id).Order());
		result.Contents[second.Id].Select(content => content.Id)
			.Should().Equal(second.Contents.Select(content => content.Id).Order());
	}

	[Fact]
	public async Task EmptySale_ReturnsEmptyDictionary()
	{
		var sale = await CreateSale(withContents: false);

		var result = await Mediator.Send(new GetSalesContentsByIdsQuery(sale.Id), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingSale_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesContentsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesContentsByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	private async Task<Sale> CreateSale(bool withContents)
	{
		var seed = TestContext.Sale;
		var transaction = await new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Transaction.SenderId)
			.WithReceiverId(TestContext.Transaction.ReceiverId)
			.WithCurrencyId(seed.CurrencyId)
			.WithAmount(20m)
			.WithSourceType(TransactionSourceType.Sale)
			.BuildAndAddToDb(Context);

		var contents = withContents
			? Enumerable.Range(0, 2).Select(_ => new SaleContentBuilder(Faker)
				.WithProductId(TestContext.Product.Id)
				.WithCurrencyId(seed.CurrencyId)
				.WithStorageContentIds([TestContext.StorageContent.Id])
				.WithCount(1).WithDetailsCount(1).Build()).ToArray()
			: Array.Empty<SaleContent>();

		return await new SaleBuilder(Faker)
			.WithUserId(TestContext.Buyer.Id)
			.WithOrganizationId(seed.OrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorageCode(seed.StorageCode)
			.WithSaleDate(DateTime.UtcNow)
			.WithContents(contents)
			.BuildAndAddToDb(Context);
	}
}
