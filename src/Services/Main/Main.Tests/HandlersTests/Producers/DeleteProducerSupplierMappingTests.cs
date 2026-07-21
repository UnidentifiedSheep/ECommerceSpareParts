using Enums;
using FluentAssertions;
using Main.Application.Handlers.ProducerSupplierMappings;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class DeleteProducerSupplierMappingTests : IntegrationTest
{
    private int _mappingId;

    public DeleteProducerSupplierMappingTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var mapping = await new ProducerSupplierMappingBuilder(Faker)
            .WithProducerId(GetContext<ProducerTestContext>().Producers[0].Id)
            .WithSupplier(Supplier.Armtek)
            .BuildAndAddToDb(Context);

        _mappingId = mapping.Id;
    }

    [Fact]
    public async Task DeleteProducerSupplierMapping_InvalidId_ThrowsProducerSupplierMappingNotFound()
    {
        var command = new DeleteProducerSupplierMappingCommand(int.MaxValue);

        await Assert.ThrowsAsync<ProducersSupplierMappingNotFoundException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteProducerSupplierMapping_Normal_Succeeds()
    {
        var command = new DeleteProducerSupplierMappingCommand(_mappingId);

        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var mappings = await Context.ProducerSupplierMappings.AsNoTracking().ToListAsync();
        mappings.Should().HaveCount(0);
    }
}
