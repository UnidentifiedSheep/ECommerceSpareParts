using System.Collections.Concurrent;
using System.Text.Json;
using Application.Common.Extensions;
using Application.Common.LRT;
using Contracts.Products;
using Contracts.Storage;
using Contracts.Job;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.Base.Synchronization;
using Main.Application.Lrts.ProductSynchronization;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Storage;

namespace Tests.LrtsTests;

public sealed class ProductSynchronizationLrtTests : IntegrationTest
{
	public ProductSynchronizationLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task Synchronization_WithMassTransit_PublishesConcreteEventTypesAndPersistsTheCursor()
	{
		var bus = Bus.Factory.CreateUsingInMemory(_ => { });
		var observer = new PublishObserver();
		using var observerHandle = bus.ConnectPublishObserver(observer);
		await bus.StartAsync(CancellationToken);
		try
		{
			var job = await ExecuteSynchronization("{}", bus);

			job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
			observer.MessageTypes.Where(type => type != typeof(JobStatusUpdatedEvent)).Should().BeEquivalentTo(new[]
			{
				typeof(ProductsUpdatedEvent),
				typeof(ProductCharacteristicsUpdatedEvent),
				typeof(ProductDimensionsUpdatedEvent),
				typeof(ProductWeightUpdatedEvent),
				typeof(ProductStocksSnapshotEvent)
			});
			var (_, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
			state!.LastProcessedId.Should().Be(TestContext.Products.Max(product => product.Id));
			state.ProcessedCount.Should().Be(TestContext.Products.Count);
		}
		finally
		{
			await bus.StopAsync(CancellationToken);
		}
	}

	[Fact]
	public async Task EmptyInput_PublishesAllSectionsAndPersistsTheCursor()
	{
		var broker = new MessageBrokerStub();

		var job = await ExecuteSynchronization("{}", broker);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		broker.PublishedMessages.Where(message => message is not JobStatusUpdatedEvent).Should().HaveCount(5);
		broker.PublishedMessagesOfType<ProductsUpdatedEvent>().Should().ContainSingle().Which.Products
			.Should().HaveCount(TestContext.Products.Count);
		broker.PublishedMessagesOfType<ProductCharacteristicsUpdatedEvent>().Should().ContainSingle();
		broker.PublishedMessagesOfType<ProductDimensionsUpdatedEvent>().Should().ContainSingle();
		broker.PublishedMessagesOfType<ProductWeightUpdatedEvent>().Should().ContainSingle();
		broker.PublishedMessagesOfType<ProductStocksSnapshotEvent>().Should().ContainSingle().Which.Products
			.Should().HaveCount(TestContext.Products.Count);
		var (input, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		input.Parts.Should().Be(ProductEventParts.All);
		state!.LastProcessedId.Should().Be(TestContext.Products.Max(product => product.Id));
		state.ProcessedCount.Should().Be(TestContext.Products.Count);
	}

	[Fact]
	public async Task ResumedSynchronization_PreservesSelectedPartsAndPublishesEmptyCharacteristics()
	{
		var products = TestContext.Products.OrderBy(product => product.Id).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.AddRange(
			ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Red"),
			ProductCharacteristic.Create(products[1].Id, characteristic.Id, "Blue"));
		await Context.SaveChangesAsync(CancellationToken);
		var broker = new MessageBrokerStub();
		var input = JsonSerializer.Serialize(new ProductSynchronizationInputState { Parts = ProductEventParts.Characteristics });

		var job = await ExecuteSynchronization(input, broker,
			new SynchronizationState<int> { LastProcessedId = products[0].Id, ProcessedCount = 1 });

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var notification = broker.PublishedMessages.Where(message => message is not JobStatusUpdatedEvent).Should().ContainSingle().Which.Should()
			.BeOfType<ProductCharacteristicsUpdatedEvent>().Which;
		notification.Products.Select(product => product.ProductId).Should().Equal(products.Skip(1).Select(product => product.Id));
		notification.Products.Single(product => product.ProductId == products[1].Id).Characteristics
			.Should().ContainSingle().Which.Value.Should().Be("Blue");
		notification.Products.Single(product => product.ProductId == products[2].Id).Characteristics.Should().BeEmpty();
		var (savedInput, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		savedInput.Parts.Should().Be(ProductEventParts.Characteristics);
		state!.LastProcessedId.Should().Be(products[^1].Id);
		state.ProcessedCount.Should().Be(products.Length);
	}

	[Theory]
	[InlineData(ProductEventParts.Stocks)]
	[InlineData(ProductEventParts.All)]
	public async Task Synchronization_ExportsFullStockSnapshotForEveryProduct(ProductEventParts parts)
	{
		var products = TestContext.Products;
		var storages = GetContext<StorageTestContext>().Storages.ToArray();
		await Context.AddRangeAsync([
			ProductStock.Create(storages[0].Code, products[0].Id, 5),
			ProductStock.Create(storages[0].Code, products[1].Id, 0),
			ProductStock.Create(storages[1].Code, products[0].Id, 7),
			ProductStock.Create(storages[1].Code, products[2].Id, 3)
		], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var broker = new MessageBrokerStub();
		var input = JsonSerializer.Serialize(new ProductSynchronizationInputState { Parts = parts });

		var job = await ExecuteSynchronization(input, broker);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var snapshot = broker.PublishedMessagesOfType<ProductStocksSnapshotEvent>().Should().ContainSingle().Which;
		snapshot.Products.Should().HaveCount(products.Count);
		var stocks = snapshot.Products.ToDictionary(product => product.ProductId, product => product.Stocks);
		stocks[products[0].Id].Should().BeEquivalentTo(new Dictionary<string, int>
		{
			[storages[0].Code] = 5,
			[storages[1].Code] = 7
		});
		stocks[products[1].Id].Should().BeEmpty();
		stocks[products[2].Id].Should().BeEquivalentTo(new Dictionary<string, int> { [storages[1].Code] = 3 });
		stocks[products[3].Id].Should().BeEmpty();
		snapshot.OccurredAt.Kind.Should().Be(DateTimeKind.Utc);
		broker.PublishedMessagesOfType<ProductStocksUpdatedEvent>().Should().BeEmpty();
		broker.PublishedMessages.Where(message => message is not ProductStocksSnapshotEvent && message is not JobStatusUpdatedEvent)
			.Should().HaveCount(parts == ProductEventParts.All ? 4 : 0);
	}

	[Fact]
	public async Task StocksOnly_WithoutStockRows_PublishesEmptyStocksAndPersistsCursor()
	{
		var broker = new MessageBrokerStub();
		var input = JsonSerializer.Serialize(new ProductSynchronizationInputState { Parts = ProductEventParts.Stocks });

		var job = await ExecuteSynchronization(input, broker);

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var snapshot = broker.PublishedMessages.Where(message => message is not JobStatusUpdatedEvent)
			.Should().ContainSingle().Which.Should().BeOfType<ProductStocksSnapshotEvent>().Which;
		snapshot.Products.Select(product => product.ProductId).Should().BeEquivalentTo(TestContext.Products.Select(product => product.Id));
		snapshot.Products.Should().OnlyContain(product => product.Stocks.Count == 0);
		var (_, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		state!.LastProcessedId.Should().Be(TestContext.Products.Max(product => product.Id));
		state.ProcessedCount.Should().Be(TestContext.Products.Count);
	}

	[Fact]
	public async Task ResumedStocksSynchronization_SkipsProductsBeforeCursor()
	{
		var products = TestContext.Products.OrderBy(product => product.Id).ToArray();
		var storage = GetContext<StorageTestContext>().Storages.First();
		await Context.AddRangeAsync([
			ProductStock.Create(storage.Code, products[0].Id, 5),
			ProductStock.Create(storage.Code, products[1].Id, 0),
			ProductStock.Create(storage.Code, products[2].Id, 7)
		], CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);
		var broker = new MessageBrokerStub();
		var input = JsonSerializer.Serialize(new ProductSynchronizationInputState { Parts = ProductEventParts.Stocks });

		var job = await ExecuteSynchronization(input, broker,
			new SynchronizationState<int> { LastProcessedId = products[0].Id, ProcessedCount = 1 });

		job.Status.Should().Be(JobStatus.Succeeded, job.ErrorMessage);
		var snapshot = broker.PublishedMessagesOfType<ProductStocksSnapshotEvent>().Should().ContainSingle().Which;
		snapshot.Products.Select(product => product.ProductId).Should().Equal(products.Skip(1).Select(product => product.Id));
		snapshot.Products.Single(product => product.ProductId == products[1].Id).Stocks.Should().BeEmpty();
		snapshot.Products.Single(product => product.ProductId == products[2].Id).Stocks.Should()
			.BeEquivalentTo(new Dictionary<string, int> { [storage.Code] = 7 });
		var (_, state) = LrtStateSerializer.Deserialize<ProductSynchronizationInputState, SynchronizationState<int>>(job.State);
		state!.LastProcessedId.Should().Be(products[^1].Id);
		state.ProcessedCount.Should().Be(products.Length);
	}

	private async Task<Job> ExecuteSynchronization(
		string input,
		IPublishEndpoint broker,
		SynchronizationState<int>? checkpoint = null)
	{
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = broker;
		var lrt = ActivatorUtilities.CreateInstance<ProductSynchronizationLrt>(Scope.ServiceProvider);
		var validatedInput = lrt.ValidateState(input);
		var jobState = checkpoint is null
			? validatedInput
			: LrtStateSerializer.Serialize(JsonSerializer.Deserialize<ProductSynchronizationInputState>(validatedInput)!, checkpoint);
		var job = SingleRunJob.Create(lrt.SystemName, jobState, 3);
		var leaseHolderId = Guid.NewGuid();
		job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));
		await Context.AddAsync(job, CancellationToken);
		await Context.SaveChangesAsync(CancellationToken);

		await lrt.ExecuteAsync(job.Id, leaseHolderId, CancellationToken);

		await Context.Entry(job).ReloadAsync(CancellationToken);
		return job;
	}

	private sealed class PublishObserver : IPublishObserver
	{
		public ConcurrentBag<Type> MessageTypes { get; } = [];

		public Task PrePublish<T>(PublishContext<T> context) where T : class => Task.CompletedTask;

		public Task PostPublish<T>(PublishContext<T> context) where T : class
		{
			MessageTypes.Add(typeof(T));
			return Task.CompletedTask;
		}

		public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class => Task.CompletedTask;
	}
}
