using System.Text.Json;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using Enums;
using FluentAssertions;
using Main.Application.Lrts.BuildCatalogueCandidates;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class BuildCatalogueCandidatesLrtTests : IntegrationTest
{
    public BuildCatalogueCandidatesLrtTests(CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task ProductsWithSameResolvedKey_CreateOneCandidateAndAssignAllProducts()
    {
        var producer = TestContext.Producers[0];
        var firstProduct = await AddSupplierProduct(
            "ABC-123",
            producer.Name,
            Supplier.Armtek);
        var secondProduct = await AddSupplierProduct(
            "ABC123",
            producer.Name,
            Supplier.Tmtr);

        var state = await ExecuteLrt();

        state.LastProcessedId.Should().Be(Math.Max(firstProduct.Id, secondProduct.Id));
        state.ProcessedRows.Should().Be(2);
        state.AssignedRows.Should().Be(2);
        state.SkippedRows.Should().Be(0);

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
        var candidate = await new CatalogueCandidateBuilder(Faker)
            .WithSku("ABC-123")
            .WithProducerId(producer.Id)
            .BuildAndAddToDb(Context);
        var supplierProduct = await AddSupplierProduct(
            "ABC123",
            producer.Name,
            Supplier.Armtek);

        var state = await ExecuteLrt();

        state.AssignedRows.Should().Be(1);
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

        var state = await ExecuteLrt();

        state.LastProcessedId.Should().Be(supplierProduct.Id);
        state.ProcessedRows.Should().Be(1);
        state.AssignedRows.Should().Be(0);
        state.SkippedRows.Should().Be(1);
        (await Context.CatalogueCandidates.CountAsync()).Should().Be(0);

        Context.ChangeTracker.Clear();
        var persistedProduct = await Context.SupplierProducts
            .AsNoTracking()
            .SingleAsync(x => x.Id == supplierProduct.Id);
        persistedProduct.CatalogueCandidateId.Should().BeNull();
    }

    private async Task<BuildCatalogueCandidatesState> ExecuteLrt()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = SingleRunJob.Create(
            BuildCatalogueCandidatesLrt.LrtSystemName,
            JsonSerializer.Serialize(new BuildCatalogueCandidatesState()));
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        await Context.AddAsync(job);
        await Context.SaveChangesAsync();

        var lrt = ActivatorUtilities.CreateInstance<BuildCatalogueCandidatesLrt>(
            Scope.ServiceProvider);

        await lrt.ExecuteAsync(job.Id, leaseHolderId);

        Context.ChangeTracker.Clear();
        var persistedJob = await Context.Jobs
            .AsNoTracking()
            .SingleAsync(x => x.Id == job.Id);
        persistedJob.Status.Should().Be(JobStatus.Succeeded);

        return JsonSerializer.Deserialize<BuildCatalogueCandidatesState>(
                   persistedJob.State)
               ?? throw new InvalidOperationException(
                   "Build catalogue candidates state could not be deserialized.");
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
