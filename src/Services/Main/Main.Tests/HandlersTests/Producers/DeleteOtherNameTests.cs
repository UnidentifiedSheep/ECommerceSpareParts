using FluentAssertions;
using Main.Application.Handlers.Producers.DeleteOtherName;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class DeleteOtherNameTests : IntegrationTest
{
    private ProducerAlias _alias = null!;

    public DeleteOtherNameTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _alias = await new ProducerOtherNameBuilder(Faker)
            .WithProducerId(GetContext<ProducerTestContext>().Producers[0].Id)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task DeleteOtherName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteOtherNameCommand(int.MaxValue, _alias.Alias);
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_UnexistingProducerOtherName_ThrowsProducersOtherNameNotFoundException()
    {
        var command =
            new DeleteOtherNameCommand(_alias.ProducerId, Faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_Normal_Succeeds()
    {
        var command =
            new DeleteOtherNameCommand(_alias.ProducerId, _alias.Alias);

        var act = () => Mediator.Send(command);
        await act.Should().NotThrowAsync();

        var dbOtherNames = await Context.ProducersOtherNames.AsNoTracking().ToListAsync();
        dbOtherNames.Should().HaveCount(0);
    }
}