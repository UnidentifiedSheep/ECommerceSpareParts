using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment.BuildCatalogueCandidatesBatch;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class BuildCatalogueCandidatesBatchTests : IntegrationTest
{
    public BuildCatalogueCandidatesBatchTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task ProductsWithSameResolvedKey_CreateOneCandidateAndAssignAllProducts()
    {
        var producer = TestContext.Producers[0];
        await AddSupplierProduct("ABC-123", producer.Name, Supplier.Armtek);
        await AddSupplierProduct("ABC123", producer.Name, Supplier.Tmtr);

        var result = await Mediator.Send(
            new BuildCatalogueCandidatesBatchCommand(0, 100));

        result.ReadRows.Should().Be(2);
        result.AssignedRows.Should().Be(2);
        result.SkippedRows.Should().Be(0);
        result.HasMore.Should().BeFalse();

        var candidate = await Context.CatalogueCandidates
            .AsNoTracking()
            .SingleAsync();
        candidate.ProducerId.Should().Be(producer.Id);
        candidate.Sku.NormalizedValue.Should().Be("ABC123");

        var candidateIds = await Context.SupplierProducts
            .AsNoTracking()
            .Select(x => x.CatalogueCandidateId)
            .ToListAsync();
        candidateIds.Should().OnlyContain(x => x == candidate.Id);
    }

    [Fact]
    public async Task ExistingCandidate_IsReused()
    {
        var producer = TestContext.Producers[0];
        var candidate = CatalogueCandidate.Create("ABC-123", producer.Id);
        await Context.AddAsync(candidate);
        await Context.SaveChangesAsync();
        var supplierProduct = await AddSupplierProduct(
            "ABC123",
            producer.Name,
            Supplier.Armtek);

        var result = await Mediator.Send(
            new BuildCatalogueCandidatesBatchCommand(0, 100));

        result.AssignedRows.Should().Be(1);
        (await Context.CatalogueCandidates.CountAsync()).Should().Be(1);

        Context.ChangeTracker.Clear();
        var persistedProduct = await Context.SupplierProducts
            .AsNoTracking()
            .SingleAsync(x => x.Id == supplierProduct.Id);
        persistedProduct.CatalogueCandidateId.Should().Be(candidate.Id);
    }

    [Fact]
    public async Task UnknownProducer_IsSkippedAndCursorStillAdvances()
    {
        var supplierProduct = await AddSupplierProduct(
            "UNKNOWN-123",
            $"unknown-{Guid.NewGuid():N}",
            Supplier.Armtek);

        var result = await Mediator.Send(
            new BuildCatalogueCandidatesBatchCommand(0, 1));

        result.LastProcessedId.Should().Be(supplierProduct.Id);
        result.ReadRows.Should().Be(1);
        result.AssignedRows.Should().Be(0);
        result.SkippedRows.Should().Be(1);
        result.HasMore.Should().BeTrue();
        (await Context.CatalogueCandidates.CountAsync()).Should().Be(0);

        Context.ChangeTracker.Clear();
        var persistedProduct = await Context.SupplierProducts
            .AsNoTracking()
            .SingleAsync(x => x.Id == supplierProduct.Id);
        persistedProduct.CatalogueCandidateId.Should().BeNull();
    }

    private Task<SupplierProduct> AddSupplierProduct(
        string sku,
        string producer,
        Supplier supplier)
    {
        return new SupplierProductBuilder(Faker)
            .WithSku(sku)
            .WithProducer(producer)
            .WithSupplier(supplier)
            .BuildAndAddToDb(Context);
    }
}
