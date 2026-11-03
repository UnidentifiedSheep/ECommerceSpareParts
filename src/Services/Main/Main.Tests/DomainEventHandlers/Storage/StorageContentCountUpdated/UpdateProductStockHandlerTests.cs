using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using FluentAssertions;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Storage;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.DomainEventHandlers.Storage.StorageContentCountUpdated;

public class UpdateProductStockHandlerTests : IntegrationTest
{
	public UpdateProductStockHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	[Fact]
	public async Task MultipleChangesAndParties_ApplyDeltasInsteadOfSummingNewCounts()
	{
		var first = NewContent(0, 0, 5);
		var second = NewContent(0, 0, 7);
		await Add(first, second);
		(await Quantity(first)).Should().Be(12);

		await Execute(async () =>
		{
			first.SetCount(8, StorageMovementType.StorageContentAddition);
			first.SetCount(4, StorageMovementType.StorageContentAddition);
			second.SetCount(2, StorageMovementType.StorageContentAddition);
			await Context.SaveChangesAsync(CancellationToken);
		});

		(await Quantity(first)).Should().Be(6);
		(await Total(first.ProductId)).Should().Be(6);
	}

	[Fact]
	public async Task ProductsAndStorages_HaveIndependentBalances()
	{
		var first = NewContent(0, 0, 4);
		var anotherProduct = NewContent(1, 0, 6);
		var anotherStorage = NewContent(0, 1, 3);

		await Add(first, anotherProduct, anotherStorage);

		(await Quantity(first)).Should().Be(4);
		(await Quantity(anotherProduct)).Should().Be(6);
		(await Quantity(anotherStorage)).Should().Be(3);
		(await Total(first.ProductId)).Should().Be(7);
		(await Total(anotherProduct.ProductId)).Should().Be(6);
	}

	[Fact]
	public async Task Transfer_UpdatesBothStoragesWhenProductDeltaIsZero()
	{
		var first = NewContent(0, 0, 5);
		var second = NewContent(0, 1, 8);
		await Add(first, second);

		await Execute(async () =>
		{
			first.SetCount(0, StorageMovementType.StorageContentAddition);
			second.SetCount(13, StorageMovementType.StorageContentAddition);
			await Context.SaveChangesAsync(CancellationToken);
		});

		(await Quantity(first)).Should().Be(0);
		(await Quantity(second)).Should().Be(13);
		(await Total(first.ProductId)).Should().Be(13);
	}

	[Fact]
	public async Task DeleteParties_SubtractsOnlyTheirRemainingQuantities()
	{
		var first = NewContent(0, 0, 5);
		var second = NewContent(0, 0, 7);
		await Add(first, second);

		await Execute(async () => { Context.Remove(first); await Context.SaveChangesAsync(CancellationToken); });
		(await Quantity(second)).Should().Be(7);
		await Execute(async () => { Context.Remove(second); await Context.SaveChangesAsync(CancellationToken); });

		(await Quantity(second)).Should().Be(0);
		(await Total(first.ProductId)).Should().Be(0);
	}

	[Fact]
	public async Task ZeroNetDelta_DoesNotCreateStockRow()
	{
		var content = NewContent(0, 0, 5);
		content.SetCount(0, StorageMovementType.StorageContentAddition);

		await Add(content);

		(await Context.ProductStocks.AnyAsync(CancellationToken)).Should().BeFalse();
		(await Total(content.ProductId)).Should().Be(0);
	}

	[Fact]
	public async Task Rollback_RestoresSourceAndBothAggregates()
	{
		var content = NewContent(0, 0, 5);
		await Add(content);
		await using (var transaction = await Context.Database.BeginTransactionAsync(CancellationToken))
		{
			await Execute(async () =>
			{
				content.SetCount(2, StorageMovementType.StorageContentAddition);
				await Context.SaveChangesAsync(CancellationToken);
			});
			(await Quantity(content)).Should().Be(2);
			await transaction.RollbackAsync(CancellationToken);
		}
		Context.ChangeTracker.Clear();

		(await Quantity(content)).Should().Be(5);
		(await Total(content.ProductId)).Should().Be(5);
		(await Context.StorageContents.AsNoTracking().SingleAsync(item => item.Id == content.Id, CancellationToken)).Count.Should().Be(5);
	}

	[Fact]
	public async Task ConcurrentBatches_CreateSingleRowWithoutLosingDeltas()
	{
		var content = NewContent(0, 0, 0);
		var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var entered = 0;
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();

		async Task Apply(int delta)
		{
			await using var transaction = factory.Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(20, 2)).ExecuteAsync(async (context, token) =>
			{
				context.GetRequiredService<IDomainEventScope>().Add(new StorageContentCountUpdatedDomainEvent(
					content.ProductId, content.StorageCode, content.CurrencyId, delta, content.BuyPrice,
					StorageMovementType.StorageContentAddition, delta));
				if (Interlocked.Increment(ref entered) == 2)
					ready.TrySetResult();
				await ready.Task.WaitAsync(token);
			}, CancellationToken);
		}

		await Task.WhenAll(Apply(4), Apply(7));

		(await Context.ProductStocks.CountAsync(CancellationToken)).Should().Be(1);
		(await Quantity(content)).Should().Be(11);
		(await Total(content.ProductId)).Should().Be(11);
	}

	[Fact]
	public async Task ConcurrentBatches_ExistingBalancesInReverseOrderDoNotLoseDeltas()
	{
		var first = NewContent(0, 0, 10);
		var second = NewContent(1, 0, 10);
		await Add(first, second);
		var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var entered = 0;
		var factory = Scope.ServiceProvider.GetRequiredService<IApplicationTransactionFactory>();

		async Task Apply(params (StorageContent Content, int Delta)[] changes)
		{
			await using var transaction = factory.Create();
			await transaction.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
			{
				var events = context.GetRequiredService<IDomainEventScope>();
				foreach (var (content, delta) in changes)
					events.Add(new StorageContentCountUpdatedDomainEvent(
						content.ProductId, content.StorageCode, content.CurrencyId, content.Count + delta,
						content.BuyPrice, StorageMovementType.StorageContentAddition, delta));
				if (Interlocked.Increment(ref entered) == 2) ready.TrySetResult();
				await ready.Task.WaitAsync(token);
			}, CancellationToken);
		}

		await Task.WhenAll(Apply((first, 2), (second, 3)), Apply((second, -2), (first, -1)));

		(await Quantity(first)).Should().Be(11);
		(await Quantity(second)).Should().Be(11);
		(await Total(first.ProductId)).Should().Be(11);
		(await Total(second.ProductId)).Should().Be(11);
	}

	[Fact]
	public async Task ExistingBalance_UpdateDoesNotChangeProductRowVersion()
	{
		var content = NewContent(0, 0, 5);
		await Add(content);
		var originalVersion = await Context.Products.AsNoTracking().Where(product => product.Id == content.ProductId)
			.Select(product => product.RowVersion).SingleAsync(CancellationToken);

		await Execute(async () =>
		{
			content.SetCount(7, StorageMovementType.StorageContentAddition);
			await Context.SaveChangesAsync(CancellationToken);
		});

		(await Quantity(content)).Should().Be(7);
		var currentVersion = await Context.Products.AsNoTracking().Where(product => product.Id == content.ProductId)
			.Select(product => product.RowVersion).SingleAsync(CancellationToken);
		currentVersion.Should().Be(originalVersion);
	}

	private StorageContent NewContent(int product, int storage, int count) => new StorageContentBuilder(Faker)
		.WithProductIds(GetContext<ProductTestContext>().Products[product].Id)
		.WithStorageCode(GetContext<StorageTestContext>().Storages.ElementAt(storage).Code)
		.WithCurrencyId(GetContext<CurrencyTestContext>().Currencies[0].Id)
		.WithBuyPrice(10m).WithCount(count).Build();

	private Task Add(params StorageContent[] contents) => Execute(async () =>
	{
		await Context.AddRangeAsync(contents, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
	});

	private Task Execute(Func<Task> action) => Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
		.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (_, _) => await action(), CancellationToken);

	private async Task<int> Quantity(StorageContent content) =>
		(await Context.ProductStocks.AsNoTracking().SingleAsync(
			stock => stock.ProductId == content.ProductId && stock.StorageCode == content.StorageCode, CancellationToken)).Quantity;

	private Task<int> Total(int productId) => Context.ProductStocks.AsNoTracking()
		.Where(stock => stock.ProductId == productId).SumAsync(stock => stock.Quantity, CancellationToken);
}
