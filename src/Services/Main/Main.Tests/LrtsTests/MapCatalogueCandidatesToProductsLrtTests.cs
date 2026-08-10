using System.Text.Json;
using Domain.CommonEntities.Job;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.MapCatalogueCandidatesToProducts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class MapCatalogueCandidatesToProductsLrtTests : IntegrationTest
{
    public MapCatalogueCandidatesToProductsLrtTests(
        CombinedContainerFixture fixture)
        : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
    }

    private ProductTestContext TestContext => GetContext<ProductTestContext>();

    [Fact]
    public async Task MatchingNormalizedSkuAndProducer_MapsCandidateToProduct()
    {
        var product = TestContext.Products[0];
        var candidate = await new CatalogueCandidateBuilder(Faker)
            .WithSku(product.Sku.Value.Insert(3, "-"))
            .WithProducerId(product.ProducerId)
            .BuildAndAddToDb(Context);

        var state = await ExecuteLrt();

        state.LastProcessedId.Should().Be(candidate.Id);
        state.ProcessedRows.Should().Be(1);
        state.MappedRows.Should().Be(1);
        state.SkippedRows.Should().Be(0);

        Context.ChangeTracker.Clear();
        var persistedCandidate = await Context.CatalogueCandidates
            .AsNoTracking()
            .SingleAsync(x => x.Id == candidate.Id);
        persistedCandidate.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task NoMatchingProduct_SkipsCandidateAndAdvancesCursor()
    {
        var candidate = await new CatalogueCandidateBuilder(Faker)
            .WithSku($"UNMATCHED-{Guid.NewGuid():N}")
            .WithProducerId(TestContext.ProducerTestContext.Producers[0].Id)
            .BuildAndAddToDb(Context);

        var state = await ExecuteLrt();

        state.LastProcessedId.Should().Be(candidate.Id);
        state.ProcessedRows.Should().Be(1);
        state.MappedRows.Should().Be(0);
        state.SkippedRows.Should().Be(1);

        Context.ChangeTracker.Clear();
        var persistedCandidate = await Context.CatalogueCandidates
            .AsNoTracking()
            .SingleAsync(x => x.Id == candidate.Id);
        persistedCandidate.ProductId.Should().BeNull();
    }

    private async Task<MapCatalogueCandidatesToProductsState> ExecuteLrt()
    {
        var leaseHolderId = Guid.NewGuid();
        var job = SingleRunJob.Create(
            MapCatalogueCandidatesToProductsLrt.LrtSystemName,
            JsonSerializer.Serialize(new MapCatalogueCandidatesToProductsState()));
        job.AcquireLease(leaseHolderId, TimeSpan.FromMinutes(5));

        await Context.AddAsync(job);
        await Context.SaveChangesAsync();

        var lrt = ActivatorUtilities
            .CreateInstance<MapCatalogueCandidatesToProductsLrt>(
                Scope.ServiceProvider);

        await lrt.ExecuteAsync(job.Id, leaseHolderId);

        Context.ChangeTracker.Clear();
        var persistedJob = await Context.Jobs
            .AsNoTracking()
            .SingleAsync(x => x.Id == job.Id);
        persistedJob.Status.Should().Be(JobStatus.Succeeded);

        return JsonSerializer.Deserialize<MapCatalogueCandidatesToProductsState>(
                   persistedJob.State)
               ?? throw new InvalidOperationException(
                   "Map catalogue candidates state could not be deserialized.");
    }
}
