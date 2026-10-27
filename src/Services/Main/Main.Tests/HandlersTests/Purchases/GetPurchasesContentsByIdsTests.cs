using FluentAssertions;
using Main.Application.Handlers.Purchases;
using Main.Entities.Purchase;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesContentsByIdsTests : IntegrationTest
{
	public GetPurchasesContentsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<PurchaseTestContext>();
	}

	private PurchaseTestContext TestContext => GetContext<PurchaseTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedContentsGroupedByPurchaseWithoutDuplicates()
	{
		var first = TestContext.Purchase;
		var second = await CreatePurchase(withContents: true);
		await CreatePurchase(withContents: true);

		var result = await Mediator.Send(
			new GetPurchasesContentsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Contents.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Contents[first.Id].Select(content => content.Id)
			.Should().Equal(first.Contents.Select(content => content.Id).Order());
		result.Contents[second.Id].Select(content => content.Id)
			.Should().Equal(second.Contents.Select(content => content.Id).Order());
	}

	[Fact]
	public async Task EmptyPurchase_ReturnsEmptyDictionary()
	{
		var purchase = await CreatePurchase(withContents: false);

		var result = await Mediator.Send(new GetPurchasesContentsByIdsQuery(purchase.Id), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingPurchase_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesContentsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesContentsByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	private async Task<Purchase> CreatePurchase(bool withContents)
	{
		var seed = TestContext.Purchase;
		var transaction = await new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Purchase.SupplierOrganizationId)
			.WithReceiverId(GetContext<UserContextTestContext>().SystemUser.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithAmount(20m)
			.WithSourceType(TransactionSourceType.Purchase)
			.BuildAndAddToDb(Context);

		var contents = new List<PurchaseContent>();
		if (withContents)
		{
			for (var index = 0; index < 2; index++)
			{
				var storageContent = await new StorageContentBuilder(Faker)
					.WithStorageCode(seed.Storage)
					.WithProductIds(TestContext.Product.Id)
					.WithCurrencyId(seed.CurrencyId)
					.WithCount(1)
					.WithBuyPrice(10m)
					.BuildAndAddToDb(Context);
				contents.Add(new PurchaseContentBuilder(Faker)
					.WithProductId(TestContext.Product.Id)
					.WithStorageContentId(storageContent.Id)
					.WithCount(1).WithPrice(10m).Build());
			}
		}

		var builder = new PurchaseBuilder(Faker)
			.WithSupplierUserId(TestContext.Supplier.Id)
			.WithSupplierOrganizationId(seed.SupplierOrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorage(seed.Storage)
			.WithPurchaseDateTime(DateTime.UtcNow);

		foreach (var content in contents)
			builder.WithContent(content);

		return await builder.BuildAndAddToDb(Context);
	}
}
