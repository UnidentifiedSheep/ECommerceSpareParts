using Application.Common.Interfaces.Lrt;
using Application.Common.Interfaces.Persistence;
using Application.Common.LRT;
using Application.Common.Models.Options.S3;
using Contracts.Products;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Locan.Core.Interfaces.Localizers;
using Main.Application.Interfaces.Persistence;
using Main.Application.Lrts.ProductCharacteristicsImport;
using Main.Entities.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using S3.Core.Interfaces;
using System.Globalization;
using System.Text.Json;
using System.Text;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.Stubs;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductCharacteristicsImportLrtTests : CsvLrtIntegrationTest<ProductCharacteristicsImportLrtTests.TestProductCharacteristicsImportLrt>
{
	public ProductCharacteristicsImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();
	private MessageBrokerStub Broker => (MessageBrokerStub)Scope.ServiceProvider
		.GetRequiredService<PublishEndpointAccessor>().Publisher;

	[Fact]
	public void CreateJob_CreatesSingleRunImport()
	{
		var dispatcher = Scope.ServiceProvider.GetRequiredService<IJobCreationDispatcher>();

		var job = dispatcher.Create(ProductCharacteristicsImportLrt.Name,
			JsonSerializer.Serialize(new ProductCharacteristicsImportInputState { FileName = "characteristics.csv" }), 3);

		job.Should().BeOfType<SingleRunJob>();
		job.SystemName.Should().Be(ProductCharacteristicsImportLrt.Name);
		job.InputInitialized.Should().BeTrue();
	}

	[Fact]
	public async Task ImportsByNormalizedName_UpdatesExistingPairsAndCanBeRepeated()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var color = Characteristic.Create("Brake Color");
		var size = Characteristic.Create("Размер детали");
		var material = Characteristic.Create("Material");
		Context.Characteristics.AddRange(color, size, material);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.AddRange(
			ProductCharacteristic.Create(products[0].Id, color.Id, "Old"),
			ProductCharacteristic.Create(products[0].Id, material.Id, "Steel"));
		await Context.SaveChangesAsync(CancellationToken);
		string[] rows =
		[
			CsvRow(products[0].Id, " BRAKE COLOR ", " Red "),
			CsvRow(products[1].Id, "brake-color", "Blue"),
			CsvRow(products[0].Id, "razmer-detali", "100 mm"),
			CsvRow(products[1].Id, "Размер детали", " 200 mm "),
			CsvRow(products[0].Id, "brake-color", "Green"),
			CsvRow(products[1].Id, "РАЗМЕР ДЕТАЛИ", "300 mm"),
			CsvRow(products[0].Id, "Brake Color", " x ")
		];

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteCsv("ProductId,Characteristic,Value", rows,
				fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
			var state = execution.GetState<ProductCharacteristicsImportState>();
			state.Checkpoint!.CurrentLine.Should().Be(7);
			state.Errors.Should().BeEmpty();
		}

		var values = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.ToDictionary(x => x.GetId(), x => x.Value);
		values.Should().HaveCount(5);
		values[(products[0].Id, color.Id)].Should().Be("x");
		values[(products[1].Id, color.Id)].Should().Be("Blue");
		values[(products[0].Id, size.Id)].Should().Be("100 mm");
		values[(products[1].Id, size.Id)].Should().Be("300 mm");
		values[(products[0].Id, material.Id)].Should().Be("Steel");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(3);
	}

	[Fact]
	public async Task InvalidRows_AreReportedWithoutDiscardingValidValues()
	{
		var productId = TestContext.Products[0].Id;
		var secondProductId = TestContext.Products[1].Id;
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(productId, "Unknown characteristic", "Red"),
				CsvRow(productId, "", "Red"),
				CsvRow("invalid", "Color", "Red"),
				CsvRow(productId, "Color", ""),
				CsvRow(productId, "Color", "   "),
				CsvRow(productId, "Color", new string('x', 129)),
				CsvRow(productId, "Color", " x "),
				CsvRow(secondProductId, "Color", " ab ")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCharacteristicsImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(8);
		state.Errors.Select(error => error.RowIdx).Should().Equal(Enumerable.Range(1, 6));
		var values = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.ToDictionary(x => x.GetId(), x => x.Value);
		values.Should().HaveCount(2);
		values[(productId, characteristic.Id)].Should().Be("x");
		values[(secondProductId, characteristic.Id)].Should().Be("ab");
		(await Context.Characteristics.CountAsync(CancellationToken)).Should().Be(1);
	}

	[Fact]
	public async Task MissingProduct_FailsTheJobAndRollsBackTheWholeBatch()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(products[0].Id, "Color", "Updated"),
				CsvRow(int.MaxValue, "Color", "Missing"),
				CsvRow(products[1].Id, "Color", "New")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.Attempts.Should().Be(1);
		execution.Job.ErrorMessage.Should().Contain("product_characteristics_product_id_fk");
		GetPublishedSnapshots().Should().BeEmpty();
		execution.GetState<ProductCharacteristicsImportState>().Checkpoint.Should().BeNull();
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(products[0].Id);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("Old");
	}

	[Fact]
	public async Task BulkUpsert_RollsBackInsertsAndUpdatesWithTheTransaction()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);
		var repository = Scope.ServiceProvider.GetRequiredService<IProductRepository>();

		await using (var transaction = await Context.Database.BeginTransactionAsync(CancellationToken))
		{
			await repository.UpsertProductCharacteristicsAsync(
			[
				ProductCharacteristic.Create(products[0].Id, characteristic.Id, "Updated"),
				ProductCharacteristic.Create(products[1].Id, characteristic.Id, "New")
			], CancellationToken);
			await transaction.RollbackAsync(CancellationToken);
		}

		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(products[0].Id);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("Old");
	}

	[Fact]
	public async Task RepeatedPairAcrossBatches_UpdatesThePersistedValueAndCheckpoint()
	{
		var productId = TestContext.Products[0].Id;
		var characteristic = Characteristic.Create("Color");
		Context.Characteristics.Add(characteristic);
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			Enumerable.Range(1, 4).Select(value => CsvRow(productId, "COLOR", value.ToString("D4", CultureInfo.InvariantCulture))),
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCharacteristicsImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(4);
		state.Errors.Should().BeEmpty();
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(productId);
		value.CharacteristicId.Should().Be(characteristic.Id);
		value.Value.Should().Be("0004");
		var snapshots = GetPublishedSnapshots();
		snapshots.Select(snapshot => snapshot.ProductId).Should().Equal(productId, productId);
		snapshots.Select(snapshot => snapshot.Characteristics.Single().Value).Should().Equal("0003", "0004");
	}

	[Fact]
	public async Task Import_PublishesCompleteSnapshotsOnlyForProductsInValidRows()
	{
		var product = TestContext.Products[0];
		var other = TestContext.Products[1];
		var color = await new CharacteristicBuilder(Faker).WithName("Color")
			.BuildAndAddToDb(Context, CancellationToken);
		var material = await new CharacteristicBuilder(Faker).WithName("Material")
			.BuildAndAddToDb(Context, CancellationToken);
		Context.ProductCharacteristics.AddRange(
			ProductCharacteristic.Create(product.Id, color.Id, "Old"),
			ProductCharacteristic.Create(product.Id, material.Id, "Steel"),
			ProductCharacteristic.Create(other.Id, color.Id, "Blue"));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(product.Id, "Color", "Red"),
				CsvRow(product.Id, "Color", "Green"),
				CsvRow(other.Id, "Unknown characteristic", "Invalid")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var snapshot = GetPublishedSnapshots().Should().ContainSingle().Which;
		snapshot.ProductId.Should().Be(product.Id);
		snapshot.Characteristics.ToDictionary(item => item.CharacteristicId, item => item.Value)
			.Should().BeEquivalentTo(new Dictionary<int, string>
			{
				[color.Id] = "Green",
				[material.Id] = "Steel"
			});
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task NoValidRows_CheckpointsWithoutPublishingProductEvents(bool invalidRow)
	{
		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			invalidRow ? [CsvRow(TestContext.Products[0].Id, "Unknown characteristic", "Invalid")] : [],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.GetState<ProductCharacteristicsImportState>().Checkpoint!.CurrentLine.Should().Be(invalidRow ? 1 : 0);
		GetPublishedSnapshots().Should().BeEmpty();
	}

	[Fact]
	public async Task PublishFailure_RollsBackInsertsUpdatesAndCheckpoint()
	{
		var products = TestContext.Products.Take(2).ToArray();
		var color = await new CharacteristicBuilder(Faker).WithName("Color")
			.BuildAndAddToDb(Context, CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[0].Id, color.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);
		var broker = Broker;
		SetPublisher(broker, _ => true);

		var execution = await ExecuteCsv(
			"ProductId,Characteristic,Value",
			[
				CsvRow(products[0].Id, "Color", "Updated"),
				CsvRow(products[1].Id, "Color", "New")
			],
			fileName => new ProductCharacteristicsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.ErrorMessage.Should().Contain("Characteristics publication failed");
		var value = (await Context.ProductCharacteristics.AsNoTracking().ToListAsync(CancellationToken))
			.Should().ContainSingle().Which;
		value.ProductId.Should().Be(products[0].Id);
		value.Value.Should().Be("Old");
		(LrtStateSerializer.DeserializeState<ProductCharacteristicsImportState>(execution.Job.State)?.Checkpoint)
			.Should().BeNull();
		GetPublishedSnapshots(broker).Should().BeEmpty();
	}

	[Fact]
	public async Task RetryAfterPublishFailure_ResumesFromCommittedBatchWithoutLosingUpdates()
	{
		var products = TestContext.Products.Take(5).ToArray();
		var color = await new CharacteristicBuilder(Faker).WithName("Color")
			.BuildAndAddToDb(Context, CancellationToken);
		Context.ProductCharacteristics.Add(ProductCharacteristic.Create(products[3].Id, color.Id, "Old"));
		await Context.SaveChangesAsync(CancellationToken);
		var broker = Broker;
		var failed = false;
		SetPublisher(broker, message =>
		{
			if (failed || message.Products.All(product => product.ProductId != products[3].Id))
				return false;
			failed = true;
			return true;
		});
		var fileName = $"{Guid.NewGuid():N}.csv";
		var committedPrefix = "ProductId,Characteristic,Value\n" + string.Join("\n",
			products.Take(3).Select(product => CsvRow(product.Id, "Color", "Committed"))) + "\n";
		var content = committedPrefix +
			CsvRow(products[3].Id, "Color", "Updated") + "\n" +
			CsvRow(products[4].Id, "Color", "New");
		var storage = Scope.ServiceProvider.GetRequiredService<S3StorageServiceStub>();
		storage.SetFile("uploads", fileName, content);

		var execution = await ExecuteLrt(
			JsonSerializer.Serialize(new ProductCharacteristicsImportInputState { FileName = fileName }),
			cancellationToken: CancellationToken);

		failed.Should().BeTrue();
		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var checkpoint = execution.GetState<ProductCharacteristicsImportState>().Checkpoint!;
		checkpoint.CurrentLine.Should().Be(5);
		checkpoint.Offset.Should().Be(Encoding.UTF8.GetByteCount(content));
		storage.GetDownloadOffsets("uploads", fileName).Should().Equal(0, Encoding.UTF8.GetByteCount(committedPrefix));
		var snapshots = GetPublishedSnapshots(broker);
		snapshots.Select(product => product.ProductId).Should().BeEquivalentTo(products.Select(product => product.Id));
		snapshots.Single(product => product.ProductId == products[3].Id).Characteristics.Single().Value.Should().Be("Updated");
		var values = await Context.ProductCharacteristics.AsNoTracking()
			.ToDictionaryAsync(item => item.ProductId, item => item.Value, CancellationToken);
		values.Should().HaveCount(5);
		values[products[3].Id].Should().Be("Updated");
		values[products[4].Id].Should().Be("New");
	}

	private void SetPublisher(MessageBrokerStub broker, Func<ProductCharacteristicsUpdatedEvent, bool> shouldFail)
	{
		var publisher = new Mock<IPublishEndpoint>();
		publisher.Setup(endpoint => endpoint.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Returns<object, CancellationToken>((message, ct) =>
			{
				if (message is ProductCharacteristicsUpdatedEvent characteristics && shouldFail(characteristics))
					throw new InvalidOperationException("Characteristics publication failed.");
				return broker.Publish(message, ct);
			});
		Scope.ServiceProvider.GetRequiredService<PublishEndpointAccessor>().Publisher = publisher.Object;
	}

	private IReadOnlyList<ProductCharacteristicsSnapshot> GetPublishedSnapshots(MessageBrokerStub? broker = null)
	{
		broker ??= Broker;
		var messages = broker.PublishedMessages.Where(message => message is
			ProductsUpdatedEvent or ProductCharacteristicsUpdatedEvent or
			ProductDimensionsUpdatedEvent or ProductWeightUpdatedEvent).ToArray();
		messages.Should().OnlyContain(message => message is ProductCharacteristicsUpdatedEvent);
		return messages.Cast<ProductCharacteristicsUpdatedEvent>().SelectMany(message => message.Products).ToArray();
	}

	public sealed class TestProductCharacteristicsImportLrt(
		IS3Service storage,
		IApplicationTransactionFactory transactions,
		IOptions<S3BucketsOptions> buckets,
		ILogger<ProductCharacteristicsImportLrt> logger,
		IContextualLocalizer localizer) : ProductCharacteristicsImportLrt(storage, transactions, buckets, logger, localizer)
	{
		protected override int BatchSize => 3;
	}
}
