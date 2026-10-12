using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.ProductCrossesImport;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class ProductCrossesImportLrtTests : CsvLrtIntegrationTest<ProductCrossesImportLrt>
{
	public ProductCrossesImportLrtTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
	}

	private ProductTestContext TestContext => GetContext<ProductTestContext>();

	[Fact]
	public async Task MissingProduct_ErrorContainsOriginalSkuAndProducer()
	{
		var existing = TestContext.Products[0];
		var producer = TestContext.ProducerTestContext.Producers.Single(x => x.Id == existing.ProducerId);
		const string missingSku = "MISSING-SKU-123";

		var execution = await ExecuteCsv(
			"Sku,Producer,CrossSku,CrossProducer",
			[CsvRow(missingSku, producer.Name, existing.Sku.Value, producer.Name)],
			fileName => new ProductCrossesImportInputState { FileName = fileName });

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var errors = execution.GetState<ProductCrossesImportState>().Errors;
		errors.Should().ContainSingle();
		errors[0].Message.Should().Contain(missingSku).And.Contain(producer.Name);
	}

	[Fact]
	public async Task ReverseDuplicate_UpsertsSingleCross()
	{
		var first = TestContext.Products[0];
		var second = TestContext.Products[1];
		var producers = TestContext.ProducerTestContext.Producers.ToDictionary(x => x.Id);

		var execution = await ExecuteCsv(
			"Sku,Producer,CrossSku,CrossProducer",
			[
				CsvRow(
					first.Sku.Value,
					producers[first.ProducerId].Name,
					second.Sku.Value,
					producers[second.ProducerId].Name),
				CsvRow(
					second.Sku.Value,
					producers[second.ProducerId].Name,
					first.Sku.Value,
					producers[first.ProducerId].Name)
			],
			fileName => new ProductCrossesImportInputState
			{
				FileName = fileName
			});

		execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		var state = execution.GetState<ProductCrossesImportState>();
		state.Errors.Should().BeEmpty();
		(await Context.ProductCrosses.AsNoTracking().CountAsync(CancellationToken)).Should().Be(1);
	}
}
