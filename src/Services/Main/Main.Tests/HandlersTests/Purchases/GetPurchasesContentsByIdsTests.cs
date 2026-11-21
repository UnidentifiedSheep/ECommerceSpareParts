using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Purchase.Purchase;
using Main.Entities.Storage.Storage;
using Main.Modules.Purchase.Application.Queries.Purchases;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesContentsByIdsTests(
	ReadScenarioFixture<GetPurchasesContentsByIdsTests.PurchasesContentsByIdsScenario> fixture)
	: ReadScenarioTest<GetPurchasesContentsByIdsTests.PurchasesContentsByIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedContentsGroupedByPurchaseWithoutDuplicates()
	{
		var first = Scenario.Get<PurchaseTestContext>().Purchase;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetPurchasesContentsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]),
			CancellationToken);

		result.Contents.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Contents[first.Id]
			.Select(content => content.Id)
			.Should()
			.Equal(first.Contents.Select(content => content.Id).Order());
		result.Contents[second.Id]
			.Select(content => content.Id)
			.Should()
			.Equal(second.Contents.Select(content => content.Id).Order());
	}

	[Fact]
	public async Task EmptyPurchase_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesContentsByIdsQuery(Scenario.Empty.Id), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingPurchase_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetPurchasesContentsByIdsQuery(Guid.NewGuid()),
			CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetPurchasesContentsByIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	public sealed class PurchasesContentsByIdsScenario : ReadScenario
	{
		private Transaction[] _transactions = [];

		private StorageContent[] _storageContents = [];

		public PurchasesContentsByIdsScenario() => RegisterBasicContext<PurchaseTestContext>();

		public Purchase Second { get; private set; } = null!;

		public Purchase Empty { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			var context = Get<PurchaseTestContext>();
			_transactions = [CreateTransaction(), CreateTransaction(), CreateTransaction()];
			_storageContents = Enumerable.Range(0, 4)
				.Select(_ => new StorageContentBuilder(Faker)
					.WithStorageCode(context.Purchase.Storage)
					.WithProductIds(context.Product.Id)
					.WithCurrencyId(context.Purchase.CurrencyId)
					.WithCount(1)
					.WithBuyPrice(10m)
					.Build())
				.ToArray();
			Context.AddRange(_transactions);
			Context.AddRange(_storageContents);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			Second = CreatePurchase(_transactions[0], CreateContents(_storageContents[..2]));
			var other = CreatePurchase(_transactions[1], CreateContents(_storageContents[2..]));
			Empty = CreatePurchase(_transactions[2]);
			return [Second, other, Empty];
		}

		private PurchaseContent[] CreateContents(IEnumerable<StorageContent> storageContents)
			=> storageContents
				.Select(storageContent => new PurchaseContentBuilder(Faker)
					.WithProductId(Get<PurchaseTestContext>().Product.Id)
					.WithStorageContentId(storageContent.Id)
					.WithCount(1)
					.WithPrice(10m)
					.Build())
				.ToArray();

		private Transaction CreateTransaction(TransactionSourceType sourceType = TransactionSourceType.Purchase)
		{
			var context = Get<PurchaseTestContext>();
			return new TransactionBuilder(Faker)
				.WithSenderId(context.Purchase.SupplierOrganizationId)
				.WithReceiverId(Get<UserContextTestContext>().SystemUser.Id)
				.WithCurrencyId(context.Purchase.CurrencyId)
				.WithAmount(20m)
				.WithSourceType(sourceType)
				.Build();
		}

		private Purchase CreatePurchase(Transaction transaction, IEnumerable<PurchaseContent>? contents = null)
		{
			var context = Get<PurchaseTestContext>();
			var builder = new PurchaseBuilder(Faker)
				.WithSupplierUserId(context.Supplier.Id)
				.WithSupplierOrganizationId(context.Purchase.SupplierOrganizationId)
				.WithTransactionId(transaction.Id)
				.WithCurrencyId(context.Purchase.CurrencyId)
				.WithStorage(context.Purchase.Storage)
				.WithPurchaseDateTime(DateTime.UtcNow);
			foreach (var content in contents ?? [])
				builder.WithContent(content);
			return builder.Build();
		}
	}
}
