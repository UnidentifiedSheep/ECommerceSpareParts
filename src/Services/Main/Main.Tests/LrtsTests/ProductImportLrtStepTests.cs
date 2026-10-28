using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProductImport;
using Main.Entities.Product.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductImportLrtStepTests : CsvLrtIntegrationTest<ProductImportLrtStep>
{
	public ProductImportLrtStepTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProducerTestContext>();
	}

	private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

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
	}

	[Fact]
	public async Task DuplicateInNextBatch_IsSkipped_AndNewProductsAreInserted()
	{
		var producer = TestContext.Producers[0];
		var rows = Enumerable.Range(1, 1000)
			.Select(index => CsvRow($"SKU-{index:D5}", $"Product {index}", producer.Name))
			.Append(CsvRow("SKU 00001", "Duplicate from next batch", producer.Name))
			.Append(CsvRow("SKU-01001", "New in next batch", producer.Name));

		var execution = await ExecuteCsv(
			"Sku,Name,Producer",
			rows,
			fileName => new ProductImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductImportState>();
		state.Errors.Should().BeEmpty();
		state.Checkpoint!.CurrentLine.Should().Be(1002);

		var products = await Context.Products.AsNoTracking().ToListAsync(CancellationToken);
		products.Should().HaveCount(1001);
		products.Should().ContainSingle(x => x.Sku.NormalizedValue == "SKU00001")
			.Which.Name.Value.Should().Be("Product 1");
		products.Should().ContainSingle(x => x.Sku.NormalizedValue == "SKU01001")
			.Which.Name.Value.Should().Be("New in next batch");
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
	}
}
