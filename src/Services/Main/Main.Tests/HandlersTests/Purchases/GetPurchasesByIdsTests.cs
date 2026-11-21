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

public sealed class GetPurchasesByIdsTests(ReadScenarioFixture<GetPurchasesByIdsTests.PurchasesByIdsScenario> fixture)
	: ReadScenarioTest<GetPurchasesByIdsTests.PurchasesByIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedPurchasesWithoutDuplicates()
	{
		var first = Scenario.First;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetPurchasesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]),
			CancellationToken);

		result.Purchases.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SinglePurchase_IsReturnedById()
	{
		var id = Scenario.Get<PurchaseTestContext>().Purchase.Id;

		var result = await Mediator.Send(new GetPurchasesByIdsQuery(id), CancellationToken);

		result.Purchases.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingPurchase_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	public sealed class PurchasesByIdsScenario : ReadScenario
	{
		private Transaction[] _transactions = [];

		public PurchasesByIdsScenario() => RegisterBasicContext<PurchaseTestContext>();

		public Purchase First { get; private set; } = null!;

		public Purchase Second { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			_transactions = [CreateTransaction(), CreateTransaction()];
			Context.AddRange(_transactions);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			First = CreatePurchase(_transactions[0]);
			Second = CreatePurchase(_transactions[1]);
			return [First, Second];
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
