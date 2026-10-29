using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Application.Common.Models.Options.S3;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Lrts.ProductImport;
using Main.Entities.Product.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using S3.Core.Interfaces;
using System.Text.Json;
using System.Text;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductImportLrtTests : CsvLrtIntegrationTest<ProductImportLrtTests.TestProductImportLrt>
{
	public ProductImportLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();
	private MessageBrokerStub Broker => (MessageBrokerStub)Scope.ServiceProvider
		.GetRequiredService<PublishEndpointAccessor>().Publisher;

	[Fact]
	public void CreateJob_CreatesSingleRunImport()
	{
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();

		var job = dispatcher.Create(nameof(ProductImportLrt),
			JsonSerializer.Serialize(new ProductImportInputState { FileName = "products.csv" }), 3);

		job.Should().BeOfType<SingleRunJob>();
		job.SystemName.Should().Be(nameof(ProductImportLrt));
		job.InputInitialized.Should().BeTrue();
	}

	[Fact]
	public async Task ExistingAndBatchDuplicates_AreSkipped_WithoutMixingProducers()
	{
		var firstProducer = TestContext.Producers[0];
		var secondProducer = TestContext.Producers[1];
		var existingProduct = await new ProductBuilder(Faker)
			.WithProducerId(firstProducer.Id)
			.WithSku(new Sku("ABC-123"))
			.BuildAndAddToDb(Context, CancellationToken);
		var existingName = existingProduct.Name.Value;

		var execution = await ExecuteCsv(
			"Sku,Name,Producer",
			[
				CsvRow(
					"ABC123",
					"Existing",
					firstProducer.Name),
				CsvRow(
					"XYZ-123",
					"New",
					firstProducer.Name),
				CsvRow(
					"XYZ 123",
					"Duplicate",
					firstProducer.Name),
				CsvRow(
					"ABC123",
					"Another producer",
					secondProducer.Name)
			],
			fileName => new ProductImportInputState
			{
				FileName = fileName
			});

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImportState>();
		state.Errors.Should().BeEmpty();
		state.Checkpoint!.CurrentLine.Should().Be(4);

		var products = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);
		products.Should().HaveCount(3);
		products.Should().ContainSingle(x => x.Id == existingProduct.Id)
			.Which.Name.Value.Should().Be(existingName);
		products
			.Should()
			.ContainSingle(x =>
				x.Sku.NormalizedValue == new Sku("XYZ123").NormalizedValue &&
				x.ProducerId == firstProducer.Id)
			.Which.Name.Value.Should().Be("New");
		products
			.Should()
			.ContainSingle(x =>
				x.Sku.NormalizedValue == new Sku("ABC123").NormalizedValue &&
				x.ProducerId == secondProducer.Id);
		AssertPublishedProducts(products.Where(product => product.Id != existingProduct.Id).Select(product => product.Id));
	}

	[Fact]
	public async Task DuplicateInNextBatch_IsSkipped_AndEachNewProductIsPublishedOnce()
	{
		var producer = TestContext.Producers[0];
		var execution = await ExecuteCsv(
			"Sku,Name,Producer",
			[
				CsvRow("SKU-001", "First product", producer.Name),
				CsvRow("SKU-002", "Second product", producer.Name),
				CsvRow("SKU 001", "Duplicate from next batch", producer.Name),
				CsvRow("SKU-003", "New in next batch", producer.Name)
			],
			fileName => new ProductImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImportState>();
		state.Errors.Should().BeEmpty();
		state.Checkpoint!.CurrentLine.Should().Be(4);

		var products = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);
		products.Should().HaveCount(3);
		products.Single(product => product.Sku.NormalizedValue == "SKU001").Name.Value
			.Should().Be("First product");
		AssertPublishedProducts(products.Select(product => product.Id));
	}

	[Fact]
	public async Task InvalidRows_DoNotPreventValidProductWithOptionalFieldsFromBeingImported()
	{
		var producer = TestContext.Producers[0];
		var group = await new ProductGroupBuilder(Faker)
			.WithName("Brake Pads")
			.BuildAndAddToDb(Context, CancellationToken);

		var execution = await ExecuteCsv(
			"Sku,Name,Producer,Description,Indicator,GroupId",
			[
				CsvRow("AB", "Too short", producer.Name, "", "", ""),
				CsvRow("VALID-123", "Valid product", "Unknown producer", "", "", ""),
				CsvRow("VALID-123", "Valid product", producer.Name, "  Description  ", "  New  ", group.Id)
			],
			fileName => new ProductImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(3);
		state.Errors.Select(error => error.RowIdx).Should().Equal(1, 2);

		var product = await Context.Products.AsNoTracking().SingleAsync(CancellationToken);
		product.Sku.Value.Should().Be("VALID-123");
		product.ProducerId.Should().Be(producer.Id);
		product.Description.Should().Be("  Description  ");
		product.Indicator!.Value.Should().Be("New");
		product.ProductGroupId.Should().Be(group.Id);
		AssertPublishedProducts([product.Id]);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task NoNewProducts_CheckpointsWithoutPublishingProductEvents(bool duplicate)
	{
		var producer = TestContext.Producers[0];
		if (duplicate)
			await new ProductBuilder(Faker).WithProducerId(producer.Id).WithSku(new Sku("EXISTING-123"))
				.BuildAndAddToDb(Context, CancellationToken);

		var execution = await ExecuteCsv(
			"Sku,Name,Producer",
			duplicate ? [CsvRow("EXISTING123", "Existing product", producer.Name)] : [],
			fileName => new ProductImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.GetState<ProductImportState>().Checkpoint!.CurrentLine.Should().Be(duplicate ? 1 : 0);
		AssertPublishedProducts([]);
	}

	[Fact]
	public async Task PublishFailure_RollsBackInsertedProductsAndCheckpoint()
	{
		var broker = Broker;
		SetPublisher(broker, _ => true);
		var producer = TestContext.Producers[0];

		var execution = await ExecuteCsv(
			"Sku,Name,Producer",
			[CsvRow("FAILED-123", "Failed batch", producer.Name)],
			fileName => new ProductImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.ErrorMessage.Should().Contain("Product publication failed");
		(await Context.Products.AnyAsync(CancellationToken)).Should().BeFalse();
		(LrtStateSerializer.DeserializeState<ProductImportState>(execution.Job.State)?.Checkpoint)
			.Should().BeNull();
		AssertPublishedProducts([], broker);
	}

	[Fact]
	public async Task RetryAfterPublishFailure_ResumesFromCommittedBatchAndPublishesEachProductOnce()
	{
		var producer = TestContext.Producers[0];
		var broker = Broker;
		var failed = false;
		SetPublisher(broker, message =>
		{
			if (failed || message.Products.All(product => product.Sku != "RETRY-003"))
				return false;
			failed = true;
			return true;
		});
		var fileName = $"{Guid.NewGuid():N}.csv";
		var committedPrefix = "Sku,Name,Producer\n" +
			CsvRow("FIRST-001", "First product", producer.Name) + "\n" +
			CsvRow("FIRST-002", "Second product", producer.Name) + "\n";
		var content = committedPrefix +
			CsvRow("RETRY-003", "Retried product", producer.Name) + "\n" +
			CsvRow("RETRY-004", "Another retried product", producer.Name);
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile("uploads", fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new ProductImportInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		failed.Should().BeTrue();
		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var checkpoint = execution.GetState<ProductImportState>().Checkpoint!;
		checkpoint.CurrentLine.Should().Be(4);
		checkpoint.Offset.Should().Be(Encoding.UTF8.GetByteCount(content));
		storage.GetDownloadOffsets("uploads", fileName).Should().Equal(0, Encoding.UTF8.GetByteCount(committedPrefix));
		var ids = await Context.Products.AsNoTracking().Select(product => product.Id).ToListAsync(CancellationToken);
		ids.Should().HaveCount(4);
		AssertPublishedProducts(ids, broker);
	}

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

	private void AssertPublishedProducts(IEnumerable<int> ids, MessageBrokerStub? broker = null)
	{
		broker ??= Broker;
		var expected = ids.ToArray();
		broker.PublishedMessagesOfType<ProductsUpdatedEvent>().SelectMany(message => message.Products)
			.Select(product => product.Id).Should().BeEquivalentTo(expected);
		broker.PublishedMessagesOfType<ProductCharacteristicsUpdatedEvent>().SelectMany(message => message.Products)
			.Select(product => product.ProductId).Should().BeEquivalentTo(expected);
		broker.PublishedMessagesOfType<ProductDimensionsUpdatedEvent>().SelectMany(message => message.Products)
			.Select(product => product.ProductId).Should().BeEquivalentTo(expected);
		broker.PublishedMessagesOfType<ProductWeightUpdatedEvent>().SelectMany(message => message.Products)
			.Select(product => product.ProductId).Should().BeEquivalentTo(expected);
	}

	public sealed class TestProductImportLrt(
		IS3Service storage,
		IApplicationTransactionFactory transactions,
		IOptions<S3BucketsOptions> buckets,
		ILogger<ProductImportLrt> logger,
		IContextualLocalizer localizer) : ProductImportLrt(storage, transactions, buckets, logger, localizer)
	{
		protected override int BatchSize => 2;
	}
}
