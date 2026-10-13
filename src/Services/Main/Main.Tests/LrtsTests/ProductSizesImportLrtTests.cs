using Domain.CommonEnums;
using Enums.Units;
using FluentAssertions;
using Main.Application.Lrts.ProductSizesImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductSizesImportLrtTests : CsvLrtIntegrationTest<ProductSizesImportLrt>
{
	public ProductSizesImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsNewSizesAndUpdatesExistingSizes_LastRowWins()
	{
		var products = TestContext.Products.Take(2).ToArray();
		Context.ProductSizes.Add(ProductSize.Create(products[0].Id, 1, 1, 1, DimensionUnit.Meter));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Unit,Length,Width,Height",
			[
				CsvRow(products[0].Id, "Meter", 2, 2, 2),
				CsvRow(products[1].Id, "Meter", 1, 2, 3),
				CsvRow(products[0].Id, "Centimeter", 20, 30, 40),
				CsvRow(products[1].Id, "Centimeter", 10, 20, 30)
			],
			fileName => new ProductSizesImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductSizesImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(4);
		state.Errors.Should().BeEmpty();

		var sizes = await Context.ProductSizes.AsNoTracking()
			.Where(x => x.ProductId == products[0].Id || x.ProductId == products[1].Id)
			.ToDictionaryAsync(x => x.ProductId, CancellationToken);
		sizes.Should().HaveCount(2);
		sizes[products[0].Id].Length.Should().Be(20);
		sizes[products[0].Id].Width.Should().Be(30);
		sizes[products[0].Id].Height.Should().Be(40);
		sizes[products[0].Id].Unit.Should().Be(DimensionUnit.Centimeter);
		sizes[products[0].Id].VolumeM3.Should().Be(0.024m);
		sizes[products[1].Id].VolumeM3.Should().Be(0.006m);
	}

	[Fact]
	public async Task InvalidRowsAndMissingProducts_AreReportedWithoutDiscardingValidSizes()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,Unit,Length,Width,Height",
			[
				CsvRow(0, "Meter", 1, 2, 3),
				CsvRow(productId, "InvalidUnit", 1, 2, 3),
				CsvRow(productId, "Meter", 0, 2, 3),
				CsvRow(int.MaxValue, "Meter", 1, 2, 3),
				CsvRow(productId, "Meter", 1, 2, 3)
			],
			fileName => new ProductSizesImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductSizesImportState>();
		state.Errors.Select(x => x.RowIdx).Should().Equal(1, 2, 3, 4);
		(await Context.ProductSizes.AsNoTracking().CountAsync(CancellationToken)).Should().Be(1);
	}
}
