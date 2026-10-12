using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProductEanImport;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductEanImportLrtTests : CsvLrtIntegrationTest<ProductEanImportLrt>
{
	public ProductEanImportLrtTests(CombinedContainerFixture fixture) : base(fixture) =>
		RegisterBasicContext<ProducerTestContext>();

	[Fact]
	public async Task ImportsEans_SkipsDuplicates_AndReportsInvalidRows()
	{
		var producerId = GetContext<ProducerTestContext>().Producers[0].Id;
		var product = await new ProductBuilder(Faker)
			.WithProducerId(producerId)
			.BuildAndAddToDb(Context);
		Context.ProductEans.Add(ProductEan.Create(product.Id, "4601234567890"));
		await Context.SaveChangesAsync(CancellationToken);

		var execution = await ExecuteCsv(
			"ProductId,Ean",
			[
				CsvRow(product.Id, "4601234567890"),
				CsvRow(product.Id, "4601234567891"),
				CsvRow(product.Id, "4601234567891"),
				CsvRow(product.Id, "123456789012345")
			],
			fileName => new ProductEanImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductEanImportState>();
		state.CurrentLine.Should().Be(4);
		state.Errors.Should().ContainSingle(error => error.RowIdx == 4);
		var eans = await Context.ProductEans.AsNoTracking()
			.Where(x => x.ProductId == product.Id)
			.Select(x => x.Ean)
			.ToListAsync(CancellationToken);
		eans.Should().BeEquivalentTo(["4601234567890", "4601234567891"]);
	}
}
