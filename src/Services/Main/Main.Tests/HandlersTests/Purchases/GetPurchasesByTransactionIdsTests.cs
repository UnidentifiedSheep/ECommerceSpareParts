using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Purchase.Purchase;
using Main.Modules.Purchase.Application.Queries.Purchases;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.TestContexts;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesByTransactionIdsTests(
	ReadScenarioFixture<GetPurchasesByTransactionIdsTests.PurchasesByTransactionIdsScenario> fixture)
	: ReadScenarioTest<GetPurchasesByTransactionIdsTests.PurchasesByTransactionIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedPurchasesKeyedByTransactionWithoutDuplicates()
	{
		var first = Scenario.Get<PurchaseTestContext>().Purchase;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery(
				[first.TransactionId, second.TransactionId, first.TransactionId, Guid.NewGuid()]),
			CancellationToken);

		result.Purchases.Keys.Should().BeEquivalentTo([first.TransactionId, second.TransactionId]);
		result.Purchases[first.TransactionId].Id.Should().Be(first.Id);
		result.Purchases[second.TransactionId].Id.Should().Be(second.Id);
	}

	[Fact]
	public async Task TransactionWithoutPurchase_IsOmitted()
	{
		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery(Scenario.ManualTransaction.Id),
			CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task ReplacedTransaction_IsOmittedAndCurrentTransactionReturnsPurchase()
	{
		var purchase = Scenario.Replaced;

		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery([Scenario.OldTransactionId, purchase.TransactionId]),
			CancellationToken);

		result.Purchases.Keys.Should().Equal(purchase.TransactionId);
		result.Purchases[purchase.TransactionId].Id.Should().Be(purchase.Id);
	}

	[Fact]
	public async Task DeletedPurchase_IsNoLongerLinkedToItsTransaction()
	{
		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery(Scenario.DeletedTransactionId),
			CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	public sealed class PurchasesByTransactionIdsScenario : ReadScenario
	{
		private Transaction[] _transactions = [];

		private Purchase _deleted = null!;

		public PurchasesByTransactionIdsScenario() => RegisterBasicContext<PurchaseTestContext>();

		public Purchase Second { get; private set; } = null!;

		public Purchase Replaced { get; private set; } = null!;

		public Guid OldTransactionId { get; private set; }

		public Guid DeletedTransactionId { get; private set; }

		public Transaction ManualTransaction { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			_transactions = Enumerable.Range(0, 5).Select(_ => CreateTransaction()).ToArray();
			ManualTransaction = CreateTransaction(TransactionSourceType.Manual);
			Context.AddRange(_transactions);
			Context.Add(ManualTransaction);
			await Context.SaveChangesAsync(CancellationToken);
			_deleted = CreatePurchase(_transactions[4]);
			Context.Add(_deleted);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			Second = CreatePurchase(_transactions[0]);
			var other = CreatePurchase(_transactions[1]);
			Replaced = CreatePurchase(_transactions[2]);
			OldTransactionId = Replaced.TransactionId;
			Replaced.SetTransactionId(_transactions[3].Id);
			DeletedTransactionId = _deleted.TransactionId;
			Context.Remove(_deleted);
			return [Second, other, Replaced];
		}

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
