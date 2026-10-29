using System.Text;
using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Application.Common.Models.Options.S3;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Lrts.MapGroupToProduct;
using Main.Entities.Product;
using Main.Entities.Product.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using S3.Core.Interfaces;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class MapGroupToProductLrtTests : CsvLrtIntegrationTest<MapGroupToProductLrtTests.TestMapGroupToProductLrt>
{
	public MapGroupToProductLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProducerTestContext>();

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();
	private MessageBrokerStub Broker => (MessageBrokerStub)Scope.ServiceProvider
		.GetRequiredService<PublishEndpointAccessor>().Publisher;

	[Fact]
	public void CreateJob_CreatesSingleRunImport()
	{
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();

		var job = dispatcher.Create(nameof(MapGroupToProductLrt),
			JsonSerializer.Serialize(new MapGroupToProductInputState { FileName = "groups.csv" }), 3);

		job.Should().BeOfType<SingleRunJob>();
		job.SystemName.Should().Be(nameof(MapGroupToProductLrt));
		job.InputInitialized.Should().BeTrue();
	}

	[Fact]
	public async Task Mapping_NormalizesKeysAndPublishesOnlyMatchedProducts_LastAssignmentWins()
	{
		var firstProducer = TestContext.Producers[0];
		var secondProducer = TestContext.Producers[1];
		var first = await CreateProduct("ABC-123", firstProducer.Id);
		var sameSku = await CreateProduct("ABC-123", secondProducer.Id);
		var sameProducer = await CreateProduct("XYZ-123", firstProducer.Id);
		var second = await CreateProduct("XYZ-123", secondProducer.Id);
		var brakes = await CreateGroup("Brakes");
		var filters = await CreateGroup("Filters");

		var execution = await ExecuteCsv(
			"Sku,Producer,ProductGroup",
			[
				CsvRow("abc 123", " " + firstProducer.Name.ToLowerInvariant() + " ", " brakes "),
				CsvRow("ABC123", firstProducer.Name, filters.Name),
				CsvRow("XYZ 123", secondProducer.Name, brakes.Name),
				CsvRow("MISSING-123", firstProducer.Name, brakes.Name),
				CsvRow("ABC123", secondProducer.Name, "Unknown group"),
				CsvRow("XYZ123", "Unknown producer", brakes.Name)
			],
			fileName => new MapGroupToProductInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<MapGroupToProductState>();
		state.Checkpoint!.CurrentLine.Should().Be(6);
		state.Errors.Select(error => error.RowIdx).Should().Equal(5, 6);
		var groups = await Context.Products.AsNoTracking()
			.ToDictionaryAsync(product => product.Id, product => product.ProductGroupId, CancellationToken);
		groups.Should().HaveCount(4);
		groups[first.Id].Should().Be(filters.Id);
		groups[second.Id].Should().Be(brakes.Id);
		groups[sameSku.Id].Should().BeNull();
		groups[sameProducer.Id].Should().BeNull();
		var snapshots = GetPublishedSnapshots();
		snapshots.Select(product => product.Id).Should().BeEquivalentTo([first.Id, second.Id]);
		snapshots.Single(product => product.Id == first.Id).ProductGroupId.Should().Be(filters.Id);
		snapshots.Single(product => product.Id == second.Id).ProductGroupId.Should().Be(brakes.Id);
	}

	[Theory]
	[InlineData("empty")]
	[InlineData("missing")]
	[InlineData("producer")]
	[InlineData("group")]
	public async Task NoMatchedProducts_CheckpointsWithoutPublishingProductEvents(string kind)
	{
		var producer = TestContext.Producers[0];
		var product = await CreateProduct("KNOWN-123", producer.Id);
		var group = await CreateGroup("Brakes");
		var rows = kind switch
		{
			"empty" => Array.Empty<string>(),
			"missing" => [CsvRow("MISSING-123", producer.Name, group.Name)],
			"producer" => [CsvRow(product.Sku.Value, "Unknown producer", group.Name)],
			_ => new[] { CsvRow(product.Sku.Value, producer.Name, "Unknown group") }
		};

		var execution = await ExecuteCsv("Sku,Producer,ProductGroup", rows,
			fileName => new MapGroupToProductInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.GetState<MapGroupToProductState>().Checkpoint!.CurrentLine.Should().Be(rows.Length);
		var saved = (await Context.Products.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		saved.ProductGroupId.Should().BeNull();
		GetPublishedSnapshots().Should().BeEmpty();
	}

	[Fact]
	public async Task RepeatedProductAcrossBatches_PublishesTheLastAssignmentOfEachBatch()
	{
		var producer = TestContext.Producers[0];
		var product = await CreateProduct("REPEATED-123", producer.Id);
		var brakes = await CreateGroup("Brakes");
		var filters = await CreateGroup("Filters");

		var execution = await ExecuteCsv("Sku,Producer,ProductGroup",
			new[] { filters, brakes, filters, brakes }
				.Select(group => CsvRow(product.Sku.Value, producer.Name, group.Name)),
			fileName => new MapGroupToProductInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.GetState<MapGroupToProductState>().Checkpoint!.CurrentLine.Should().Be(4);
		var snapshots = GetPublishedSnapshots();
		snapshots.Select(snapshot => snapshot.Id).Should().Equal(product.Id, product.Id);
		snapshots.Select(snapshot => snapshot.ProductGroupId).Should().Equal(filters.Id, brakes.Id);
		(await Context.Products.AsNoTracking().SingleAsync(CancellationToken)).ProductGroupId.Should().Be(brakes.Id);
	}

	[Fact]
	public async Task PublishFailure_RollsBackAssignmentsAndCheckpoint()
	{
		var producer = TestContext.Producers[0];
		var first = await CreateProduct("FIRST-123", producer.Id);
		var second = await CreateProduct("SECOND-123", producer.Id);
		var brakes = await CreateGroup("Brakes");
		var filters = await CreateGroup("Filters");
		first.SetProductGroup(brakes.Id);
		await Context.SaveChangesAsync(CancellationToken);
		var broker = Broker;
		SetPublisher(broker, _ => true);

		var execution = await ExecuteCsv("Sku,Producer,ProductGroup",
			[
				CsvRow(first.Sku.Value, producer.Name, filters.Name),
				CsvRow(second.Sku.Value, producer.Name, filters.Name)
			],
			fileName => new MapGroupToProductInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.ErrorMessage.Should().Contain("Product publication failed");
		var groups = await Context.Products.AsNoTracking()
			.ToDictionaryAsync(product => product.Id, product => product.ProductGroupId, CancellationToken);
		groups[first.Id].Should().Be(brakes.Id);
		groups[second.Id].Should().BeNull();
		(LrtStateSerializer.DeserializeState<MapGroupToProductState>(execution.Job.State)?.Checkpoint)
			.Should().BeNull();
		GetPublishedSnapshots(broker).Should().BeEmpty();
	}

	[Fact]
	public async Task RetryAfterPublishFailure_ResumesFromCommittedBatchWithoutLosingAssignments()
	{
		var producer = TestContext.Producers[0];
		var products = new List<Product>();
		for (var i = 1; i <= 5; i++)
			products.Add(await CreateProduct($"SKU-{i:D3}", producer.Id));
		var brakes = await CreateGroup("Brakes");
		var filters = await CreateGroup("Filters");
		var broker = Broker;
		var failed = false;
		SetPublisher(broker, message =>
		{
			if (failed || message.Products.All(product => product.Id != products[3].Id))
				return false;
			failed = true;
			return true;
		});
		var fileName = $"{Guid.NewGuid():N}.csv";
		var committedPrefix = "Sku,Producer,ProductGroup\n" + string.Join("\n",
			products.Take(3).Select(product => CsvRow(product.Sku.Value, producer.Name, brakes.Name))) + "\n";
		var content = committedPrefix + string.Join("\n",
			products.Skip(3).Select(product => CsvRow(product.Sku.Value, producer.Name, filters.Name)));
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile("uploads", fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new MapGroupToProductInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		failed.Should().BeTrue();
		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var checkpoint = execution.GetState<MapGroupToProductState>().Checkpoint!;
		checkpoint.CurrentLine.Should().Be(5);
		checkpoint.Offset.Should().Be(Encoding.UTF8.GetByteCount(content));
		storage.GetDownloadOffsets("uploads", fileName).Should().Equal(0, Encoding.UTF8.GetByteCount(committedPrefix));
		var snapshots = GetPublishedSnapshots(broker);
		snapshots.Select(snapshot => snapshot.Id).Should().BeEquivalentTo(products.Select(product => product.Id));
		var groups = await Context.Products.AsNoTracking()
			.ToDictionaryAsync(product => product.Id, product => product.ProductGroupId, CancellationToken);
		foreach (var product in products.Take(3))
			groups[product.Id].Should().Be(brakes.Id);
		foreach (var product in products.Skip(3))
			groups[product.Id].Should().Be(filters.Id);
	}

	private Task<Product> CreateProduct(string sku, int producerId) => new ProductBuilder(Faker)
		.WithProducerId(producerId).WithSku(new Sku(sku)).BuildAndAddToDb(Context, CancellationToken);

	private Task<ProductGroup> CreateGroup(string name) => new ProductGroupBuilder(Faker)
		.WithName(name).BuildAndAddToDb(Context, CancellationToken);

	private void SetPublisher(MessageBrokerStub broker, Func<ProductsUpdatedEvent, bool> shouldFail)
	{
		var publisher = new Mock<IPublishEndpoint>();
		publisher.Setup(endpoint => endpoint.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Returns<object, CancellationToken>((message, ct) =>
			{
				if (message is ProductsUpdatedEvent products && shouldFail(products))
					throw new InvalidOperationException("Product publication failed.");
				return broker.Publish(message, ct);
			});
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = publisher.Object;
	}

	private IReadOnlyList<ProductSnapshot> GetPublishedSnapshots(MessageBrokerStub? broker = null)
	{
		broker ??= Broker;
		var messages = broker.PublishedMessages.Where(message => message is
			ProductsUpdatedEvent or ProductCharacteristicsUpdatedEvent or
			ProductDimensionsUpdatedEvent or ProductWeightUpdatedEvent).ToArray();
		messages.Should().OnlyContain(message => message is ProductsUpdatedEvent);
		return messages.Cast<ProductsUpdatedEvent>().SelectMany(message => message.Products).ToArray();
	}

	public sealed class TestMapGroupToProductLrt(
		IS3Service storage,
		IApplicationTransactionFactory transactions,
		IOptions<S3BucketsOptions> buckets,
		ILogger<MapGroupToProductLrt> logger,
		IContextualLocalizer localizer) : MapGroupToProductLrt(storage, transactions, buckets, logger, localizer)
	{
		protected override int BatchSize => 3;
	}
}
