using Abstractions.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProducerSupplierMappings.GetProducerSupplierMappings;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class GetProducerSupplierMappingsTests : IntegrationTest
{
    public GetProducerSupplierMappingsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Fact]
    public async Task GetProducerSupplierMappings_ByProducer_ReturnsOnlyProducerMappings()
    {
        var producer = TestContext.Producers[0];
        var anotherProducer = TestContext.Producers[1];
        var expected = await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.Armtek)
            .WithSupplierProducerName("Armtek producer")
            .BuildAndAddToDb(Context);
        await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(anotherProducer.Id)
            .WithSupplier(Supplier.Armtek)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetProducerSupplierMappingsQuery(
                producer.Id,
                [],
                new Pagination(0, 10)));

        result.Mappings.Should().ContainSingle();
        result.Mappings[0].Id.Should().Be(expected.Id);
        result.Mappings[0].ProducerId.Should().Be(producer.Id);
        result.Mappings[0].Supplier.Should().Be(expected.Supplier);
        result.Mappings[0].SupplierProducerName.Should().Be(expected.SupplierProducerName);
    }

    [Fact]
    public async Task GetProducerSupplierMappings_BySupplier_ReturnsOnlyRequestedSuppliers()
    {
        var producer = TestContext.Producers[0];
        var expected = await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.Armtek)
            .BuildAndAddToDb(Context);
        await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.FavoritParts)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetProducerSupplierMappingsQuery(
                producer.Id,
                [Supplier.Armtek],
                new Pagination(0, 10)));

        result.Mappings.Should().ContainSingle();
        result.Mappings[0].Id.Should().Be(expected.Id);
        result.Mappings[0].Supplier.Should().Be(Supplier.Armtek);
    }

    [Fact]
    public async Task GetProducerSupplierMappings_WithPagination_ReturnsRequestedPage()
    {
        var producer = TestContext.Producers[0];
        await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.Armtek)
            .BuildAndAddToDb(Context);
        await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(producer.Id)
            .WithSupplier(Supplier.FavoritParts)
            .BuildAndAddToDb(Context);

        var result = await Mediator.Send(
            new GetProducerSupplierMappingsQuery(
                producer.Id,
                [],
                new Pagination(0, 1)));

        result.Mappings.Should().ContainSingle();
    }

    [Fact]
    public async Task GetProducerSupplierMappings_InvalidPagination_ThrowsValidationException()
    {
        var query = new GetProducerSupplierMappingsQuery(
            TestContext.Producers[0].Id,
            [],
            new Pagination(-1, 10));

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(query));
    }
}
