using Domain.CommonEnums;
using Enums.Units;
using FluentAssertions;
using Main.Application.Lrts.ProductWeightImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductWeightImportLrtTests : CsvLrtIntegrationTest<ProductWeightImportLrt>
{
	public ProductWeightImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsNewWeightsAndUpdatesExistingWeights_LastValidRowWins()
	{
		var products = TestContext.Products.Take(2).ToArray();
		Context.ProductWeights.Add(ProductWeight.Create(products[0].Id, 1, WeightUnit.Kilogram));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Unit,Weight",
			[
				CsvRow(products[0].Id, "Gram", "500.25"),
				CsvRow(products[1].Id, "Kilogram", "2.50"),
				CsvRow(products[0].Id, "Kilogram", "1.75"),
				CsvRow(products[1].Id, "Tonne", "0.02"),
				CsvRow(products[1].Id, "Gram", "-1")
			],
			fileName => new ProductWeightImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductWeightImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(5);
		state.Errors.Should().ContainSingle(error => error.RowIdx == 5);
		var weights = await Context.ProductWeights.AsNoTracking().ToDictionaryAsync(x => x.ProductId, CancellationToken);
		weights.Should().HaveCount(2);
		weights[products[0].Id].Weight.Should().Be(1.75m);
		weights[products[0].Id].Unit.Should().Be(WeightUnit.Kilogram);
		weights[products[1].Id].Weight.Should().Be(0.02m);
		weights[products[1].Id].Unit.Should().Be(WeightUnit.Tonne);
	}

	[Fact]
	public async Task InvalidRowsAndMissingProducts_AreReportedWithoutDiscardingValidWeights()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,Unit,Weight",
			[
				CsvRow(0, "Gram", "1"),
				CsvRow(-1, "Gram", "1"),
				CsvRow(productId, "InvalidUnit", "1"),
				CsvRow(productId, "999", "1"),
				CsvRow(productId, "Kilogram", "0"),
				CsvRow(productId, "Kilogram", "-1"),
				CsvRow(productId, "Kilogram", "1.234"),
				CsvRow(productId, "Kilogram", "invalid"),
				CsvRow(int.MaxValue, "Kilogram", "1"),
				CsvRow(productId, "Gram", "12.34")
			],
			fileName => new ProductWeightImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductWeightImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(10);
		state.Errors.Select(error => error.RowIdx).Should().BeEquivalentTo(Enumerable.Range(1, 9));
		var weight = (await Context.ProductWeights.AsNoTracking().ToListAsync(CancellationToken)).Should().ContainSingle().Which;
		weight.ProductId.Should().Be(productId);
		weight.Weight.Should().Be(12.34m);
		weight.Unit.Should().Be(WeightUnit.Gram);
	}

	[Fact]
	public async Task RepeatedProductAcrossBatches_UpdatesThePersistedWeightAndCheckpoint()
	{
		var productId = TestContext.Products[0].Id;
		var execution = await ExecuteCsv(
			"ProductId,Unit,Weight",
			Enumerable.Range(1, 1001).Select(weight => CsvRow(productId, "Kilogram", weight)),
			fileName => new ProductWeightImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductWeightImportState>();
		state.Checkpoint!.CurrentLine.Should().Be(1001);
		state.Errors.Should().BeEmpty();
		var weight = (await Context.ProductWeights.AsNoTracking().ToListAsync(CancellationToken)).Should().ContainSingle().Which;
		weight.ProductId.Should().Be(productId);
		weight.Weight.Should().Be(1001);
		weight.Unit.Should().Be(WeightUnit.Kilogram);
	}
}
