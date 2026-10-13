using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProductCrossesByIdsImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductCrossesByIdsImportLrtTests : CsvLrtIntegrationTest<ProductCrossesByIdsImportLrt>
{
	public ProductCrossesByIdsImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task ImportsCrosses_SkipsExistingAndReverseDuplicates()
	{
		var products = TestContext.Products.Take(3).ToArray();
		Context.ProductCrosses.Add(ProductCross.Create(products[0].Id, products[1].Id));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"Id,CrossId",
			[
				CsvRow(products[1].Id, products[0].Id),
				CsvRow(products[0].Id, products[2].Id),
				CsvRow(products[2].Id, products[0].Id)
			],
			fileName => new ProductCrossesByIdsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCrossesByIdsImportState>();
		state.CurrentLine.Should().Be(3);
		state.Errors.Should().BeEmpty();
		var crosses = await Context.ProductCrosses.AsNoTracking()
			.Select(x => new { x.LeftProductId, x.RightProductId })
			.ToListAsync(CancellationToken);
		crosses.Should().HaveCount(2);
		crosses.Should().Contain(x => x.LeftProductId == Math.Min(products[0].Id, products[1].Id)
			&& x.RightProductId == Math.Max(products[0].Id, products[1].Id));
		crosses.Should().Contain(x => x.LeftProductId == Math.Min(products[0].Id, products[2].Id)
			&& x.RightProductId == Math.Max(products[0].Id, products[2].Id));
	}

	[Fact]
	public async Task InvalidRows_AreReportedAndValidCrossIsImported()
	{
		var products = TestContext.Products.Take(2).ToArray();

		var execution = await ExecuteCsv(
			"Id,CrossId",
			[
				CsvRow(0, products[1].Id),
				CsvRow(products[0].Id, products[0].Id),
				CsvRow(products[0].Id, "not-an-id"),
				CsvRow(products[0].Id, products[1].Id)
			],
			fileName => new ProductCrossesByIdsImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var errors = execution.GetState<ProductCrossesByIdsImportState>().Errors;
		errors.Select(x => x.RowIdx).Should().Equal(1, 2, 3);
		(await Context.ProductCrosses.AsNoTracking().CountAsync(CancellationToken)).Should().Be(1);
	}
}
