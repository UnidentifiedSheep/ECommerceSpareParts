using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Interfaces.Products;
using Main.Application.Lrts.ProductCrossesByIdsDelete;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductCrossesByIdsDeleteLrtTests : CsvLrtIntegrationTest<ProductCrossesByIdsDeleteLrt>
{
	public ProductCrossesByIdsDeleteLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProductTestContext>();

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task DeletesExistingCrosses_SkipsMissingAndReverseDuplicates_AndInvalidatesCache()
	{
		var products = TestContext.Products.Take(4).ToArray();
		var deletedCross = ProductCross.Create(products[0].Id, products[1].Id);
		var retainedCross = ProductCross.Create(products[0].Id, products[2].Id);
		Context.ProductCrosses.AddRange(deletedCross, retainedCross);
		await Context.SaveChangesAsync(CancellationToken);

		var productProvider = Scope.ServiceProvider.GetRequiredService<IProductProvider>();
		(await productProvider.GetProductCrossesAsync(products[0].Id, null, CancellationToken))
			.Should().Contain(products[1].Id);

		var execution = await ExecuteCsv(
			"Id,CrossId",
			[
				CsvRow(products[1].Id, products[0].Id),
				CsvRow(products[0].Id, products[1].Id),
				CsvRow(products[0].Id, products[3].Id),
				CsvRow(products[0].Id, int.MaxValue)
			],
			fileName => new ProductCrossesByIdsDeleteInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCrossesByIdsDeleteState>();
		state.CurrentLine.Should().Be(4);
		state.Errors.Should().BeEmpty();
		var crosses = await Context.ProductCrosses.AsNoTracking()
			.Select(x => new { x.LeftProductId, x.RightProductId })
			.ToListAsync(CancellationToken);
		crosses.Should().ContainSingle();
		crosses[0].LeftProductId.Should().Be(retainedCross.LeftProductId);
		crosses[0].RightProductId.Should().Be(retainedCross.RightProductId);

		var cachedCrossIds = await productProvider.GetProductCrossesAsync(
			products[0].Id, null, CancellationToken);
		cachedCrossIds.Should().BeEquivalentTo([products[2].Id]);
	}

	[Fact]
	public async Task InvalidRows_AreReportedAndValidCrossIsDeleted()
	{
		var products = TestContext.Products.Take(2).ToArray();
		Context.ProductCrosses.Add(ProductCross.Create(products[0].Id, products[1].Id));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"Id,CrossId",
			[
				CsvRow(0, products[1].Id),
				CsvRow(products[0].Id, products[0].Id),
				CsvRow(products[0].Id, "not-an-id"),
				CsvRow(products[0].Id, products[1].Id)
			],
			fileName => new ProductCrossesByIdsDeleteInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		execution.GetState<ProductCrossesByIdsDeleteState>().Errors
			.Select(x => x.RowIdx).Should().Equal(1, 2, 3);
		(await Context.ProductCrosses.AsNoTracking().CountAsync(CancellationToken)).Should().Be(0);
	}
}
