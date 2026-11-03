using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Contracts.Storage;
using FluentAssertions;
using Main.Entities.DomainEvents.StorageContent;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Storage;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.DomainEventHandlers.Storage.ProductStockUpdated;

public class PublishIntegrationEventsHandlerTests : IntegrationTest
{
	public PublishIntegrationEventsHandlerTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
	}

	private MessageBrokerStub Broker => (MessageBrokerStub)Scope.ServiceProvider
		.GetRequiredService<PublishEndpointAccessor>().Publisher;

	[Fact]
	public async Task NewParties_PublishCurrentTotalsGroupedByStorage()
	{
		var first = NewContent(5);
		var second = NewContent(7);
		var anotherProduct = NewContent(4, product: 1);
		var anotherStorage = NewContent(3, storage: 1);
		var before = Broker.PublishedMessages.Count;

		await Add(first, second, anotherProduct, anotherStorage);

		var messages = PublishedSince(before);
		messages.Should().HaveCount(2);
		messages.Single(message => message.StorageCode == first.StorageCode).Items
			.Select(item => (item.ProductId, item.Quantity)).Should()
			.BeEquivalentTo([(first.ProductId, 12), (anotherProduct.ProductId, 4)]);
		messages.Single(message => message.StorageCode == anotherStorage.StorageCode).Items
			.Should().ContainSingle().Which.Quantity.Should().Be(3);
		messages.Should().OnlyContain(message => message.OccurredAt.Kind == DateTimeKind.Utc);
	}

	[Fact]
	public async Task RepeatedChanges_PublishFinalTotalInsteadOfDeltaOrPartyQuantity()
	{
		var first = NewContent(5);
		var second = NewContent(7);
		await Add(first, second);
		var before = Broker.PublishedMessages.Count;

		await Execute(async (context, token) =>
		{
			first.SetCount(8, StorageMovementType.StorageContentAddition);
			first.SetCount(10, StorageMovementType.StorageContentAddition);
			second.SetCount(9, StorageMovementType.StorageContentAddition);
			await context.UnitOfWork.SaveChangesAsync(token);
		});

		var message = PublishedSince(before).Should().ContainSingle().Which;
		message.StorageCode.Should().Be(first.StorageCode);
		var item = message.Items.Should().ContainSingle().Which;
		item.ProductId.Should().Be(first.ProductId);
		item.Quantity.Should().Be(19);
		(await Context.ProductStocks.AsNoTracking().SingleAsync(CancellationToken)).Quantity.Should().Be(item.Quantity);
	}

	[Fact]
	public async Task DeletedLastParty_PublishesZeroQuantity()
	{
		var content = NewContent(5);
		await Add(content);
		var before = Broker.PublishedMessages.Count;

		await Execute(async (context, token) =>
		{
			Context.Remove(content);
			await context.UnitOfWork.SaveChangesAsync(token);
		});

		var message = PublishedSince(before).Should().ContainSingle().Which;
		message.StorageCode.Should().Be(content.StorageCode);
		var item = message.Items.Should().ContainSingle().Which;
		item.ProductId.Should().Be(content.ProductId);
		item.Quantity.Should().Be(0);
	}

	[Fact]
	public async Task ZeroDelta_DoesNotPublishStockUpdate()
	{
		var content = NewContent(5);
		await Add(content);
		var before = Broker.PublishedMessages.Count;

		await Execute((context, _) =>
		{
			context.GetRequiredService<IDomainEventScope>().Add(new StorageContentCountUpdatedDomainEvent(
				content.ProductId, content.StorageCode, content.CurrencyId, content.Count,
				content.BuyPrice, StorageMovementType.StorageContentAddition, 0));
			return Task.CompletedTask;
		});

		PublishedSince(before).Should().BeEmpty();
	}

	[Fact]
	public async Task FailedOperation_DoesNotPublishOrPersistStockUpdate()
	{
		var content = NewContent(5);
		await Add(content);
		var before = Broker.PublishedMessages.Count;

		await Assert.ThrowsAsync<InvalidOperationException>(() => Execute(async (context, token) =>
		{
			content.SetCount(8, StorageMovementType.StorageContentAddition);
			await context.UnitOfWork.SaveChangesAsync(token);
			throw new InvalidOperationException("Operation failed.");
		}));

		PublishedSince(before).Should().BeEmpty();
		(await Context.ProductStocks.AsNoTracking().SingleAsync(CancellationToken)).Quantity.Should().Be(5);
		(await Context.StorageContents.AsNoTracking().SingleAsync(CancellationToken)).Count.Should().Be(5);
	}

	private IReadOnlyList<ProductStocksUpdatedEvent> PublishedSince(int before) =>
		Broker.PublishedMessages.Skip(before).OfType<ProductStocksUpdatedEvent>().ToArray();

	private StorageContent NewContent(int count, int product = 0, int storage = 0) => new StorageContentBuilder(Faker)
		.WithProductIds(GetContext<ProductTestContext>().Products[product].Id)
		.WithStorageCode(GetContext<StorageTestContext>().Storages.ElementAt(storage).Code)
		.WithCurrencyId(GetContext<CurrencyTestContext>().Currencies[0].Id)
		.WithBuyPrice(10m).WithCount(count).Build();

	private Task Add(params StorageContent[] contents) => Execute(async (context, token) =>
	{
		await context.UnitOfWork.AddRangeAsync(contents, token);
		await context.UnitOfWork.SaveChangesAsync(token);
	});

	private Task Execute(Func<IApplicationTransactionContext, CancellationToken, Task> action) =>
		Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(action, CancellationToken);
}
