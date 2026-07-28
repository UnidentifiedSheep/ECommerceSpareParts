using Abstractions.Models;
using FluentAssertions;
using Main.Application.Handlers.Producers.GetProducers;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;

namespace Tests.HandlersTests.Producers;

public class GetProducersTests(CombinedContainerFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task GetProducers_WithMultipleIds_ReturnsOnlyRequestedProducers()
    {
        var first = await new ProducerBuilder(Faker)
            .WithName("First requested producer")
            .BuildAndAddToDb(Context);
        var second = await new ProducerBuilder(Faker)
            .WithName("Second requested producer")
            .BuildAndAddToDb(Context);
        var skipped = await new ProducerBuilder(Faker)
            .WithName("Skipped producer")
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            CreateQuery([first.Id, second.Id]));

        result.Producers
            .Select(x => x.Id)
            .Should()
            .BeEquivalentTo([first.Id, second.Id]);
        result.Producers.Should().NotContain(x => x.Id == skipped.Id);
    }

    [Fact]
    public async Task GetProducers_WithEmptyIds_DoesNotFilterById()
    {
        var first = await new ProducerBuilder(Faker)
            .WithName("First producer")
            .BuildAndAddToDb(Context);
        var second = await new ProducerBuilder(Faker)
            .WithName("Second producer")
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(CreateQuery([]));

        result.Producers.Should().Contain(x => x.Id == first.Id);
        result.Producers.Should().Contain(x => x.Id == second.Id);
    }

    [Fact]
    public async Task GetProducers_WithDuplicateAndMissingIds_ReturnsExistingProducerOnce()
    {
        var producer = await new ProducerBuilder(Faker)
            .WithName("Existing producer")
            .BuildAndAddToDb(Context);
        var skipped = await new ProducerBuilder(Faker)
            .WithName("Skipped producer")
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            CreateQuery([producer.Id, producer.Id, int.MaxValue]));

        result.Producers.Should().ContainSingle();
        result.Producers.Single().Id.Should().Be(producer.Id);
        result.Producers.Should().NotContain(x => x.Id == skipped.Id);
    }

    [Fact]
    public async Task GetProducers_WithIdsAndSearchTerm_AppliesBothFilters()
    {
        var matchingRequested = await new ProducerBuilder(Faker)
            .WithName("Alpha requested producer")
            .BuildAndAddToDb(Context);
        var notMatchingRequested = await new ProducerBuilder(Faker)
            .WithName("Beta requested producer")
            .BuildAndAddToDb(Context);
        var matchingNotRequested = await new ProducerBuilder(Faker)
            .WithName("Alpha skipped producer")
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            CreateQuery(
                [matchingRequested.Id, notMatchingRequested.Id],
                searchTerm: "Alpha"));

        result.Producers.Should().ContainSingle();
        result.Producers.Single().Id.Should().Be(matchingRequested.Id);
        result.Producers.Should().NotContain(x => x.Id == notMatchingRequested.Id);
        result.Producers.Should().NotContain(x => x.Id == matchingNotRequested.Id);
    }

    [Fact]
    public async Task GetProducers_WithIds_AppliesPaginationAfterFiltering()
    {
        var first = await new ProducerBuilder(Faker)
            .WithName("Alpha producer")
            .BuildAndAddToDb(Context);
        var second = await new ProducerBuilder(Faker)
            .WithName("Beta producer")
            .BuildAndAddToDb(Context);
        var third = await new ProducerBuilder(Faker)
            .WithName("Gamma producer")
            .BuildAndAddToDb(Context);
        await new ProducerBuilder(Faker)
            .WithName("Aardvark skipped producer")
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            CreateQuery(
                [first.Id, second.Id, third.Id],
                page: 1,
                size: 1));

        result.Producers.Should().ContainSingle();
        result.Producers.Single().Id.Should().Be(second.Id);
    }

    private static GetProducersQuery CreateQuery(
        IEnumerable<int> ids,
        string? searchTerm = null,
        int page = 0,
        int size = 100)
    {
        return new GetProducersQuery(
            searchTerm,
            ids,
            new Pagination(page, size));
    }
}
