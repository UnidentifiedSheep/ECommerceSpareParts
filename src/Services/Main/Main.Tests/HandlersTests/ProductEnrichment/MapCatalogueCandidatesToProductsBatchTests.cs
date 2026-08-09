using FluentAssertions;
using Main.Application.Handlers.ProductEnrichment.MapCatalogueCandidatesToProductsBatch;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.ProductEnrichment;

public sealed class MapCatalogueCandidatesToProductsBatchTests : IntegrationTest
{
    public MapCatalogueCandidatesToProductsBatchTests(
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

        var result = await Mediator.Send(
            new MapCatalogueCandidatesToProductsBatchCommand(0, 100));

        result.LastProcessedId.Should().Be(candidate.Id);
        result.ReadRows.Should().Be(1);
        result.MappedRows.Should().Be(1);
        result.SkippedRows.Should().Be(0);
        result.HasMore.Should().BeFalse();

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

        var result = await Mediator.Send(
            new MapCatalogueCandidatesToProductsBatchCommand(0, 1));

        result.LastProcessedId.Should().Be(candidate.Id);
        result.ReadRows.Should().Be(1);
        result.MappedRows.Should().Be(0);
        result.SkippedRows.Should().Be(1);
        result.HasMore.Should().BeTrue();

        Context.ChangeTracker.Clear();
        var persistedCandidate = await Context.CatalogueCandidates
            .AsNoTracking()
            .SingleAsync(x => x.Id == candidate.Id);
        persistedCandidate.ProductId.Should().BeNull();
    }
}
