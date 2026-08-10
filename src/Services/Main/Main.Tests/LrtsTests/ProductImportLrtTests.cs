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

public sealed class ProductImportLrtTests
    : CsvLrtIntegrationTest<ProductImportLrt>
{
    public ProductImportLrtTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task ExistingAndBatchDuplicates_AreSkipped_WithoutMixingProducers()
    {
        var firstProducer = TestContext.Producers[0];
        var secondProducer = TestContext.Producers[1];
        await new ProductBuilder(Faker)
            .WithProducerId(firstProducer.Id)
            .WithSku(new Sku("ABC-123"))
            .BuildAndAddToDb(Context);

        var execution = await ExecuteCsv(
            "Sku,Name,Producer",
            [
                CsvRow("ABC123", "Existing", firstProducer.Name),
                CsvRow("XYZ-123", "New", firstProducer.Name),
                CsvRow("XYZ 123", "Duplicate", firstProducer.Name),
                CsvRow("ABC123", "Another producer", secondProducer.Name)
            ],
            fileName => new ProductImportInputState { FileName = fileName });

        execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
        var state = execution.GetState<ProductImportState>();
        state.Errors.Should().BeEmpty();
        state.SkippedLines.Should().BeEquivalentTo([1, 3]);

        var products = await Context.Products.AsNoTracking().ToListAsync();
        products.Should().HaveCount(3);
        products.Should().ContainSingle(x =>
            x.Sku.NormalizedValue == new Sku("XYZ123").NormalizedValue &&
            x.ProducerId == firstProducer.Id);
        products.Should().ContainSingle(x =>
            x.Sku.NormalizedValue == new Sku("ABC123").NormalizedValue &&
            x.ProducerId == secondProducer.Id);
    }
}
