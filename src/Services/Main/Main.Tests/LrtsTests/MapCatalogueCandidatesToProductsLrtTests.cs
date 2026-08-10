using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.MapCatalogueCandidatesToProducts;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.LrtsTests;

public sealed class MapCatalogueCandidatesToProductsLrtTests
    : LrtIntegrationTest<MapCatalogueCandidatesToProductsLrt>
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

        var execution = await ExecuteLrt();
        var state = execution.GetState<MapCatalogueCandidatesToProductsState>();

        execution.Job.Status.Should().Be(JobStatus.Succeeded);
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

        var execution = await ExecuteLrt();
        var state = execution.GetState<MapCatalogueCandidatesToProductsState>();

        execution.Job.Status.Should().Be(JobStatus.Succeeded);
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

}
