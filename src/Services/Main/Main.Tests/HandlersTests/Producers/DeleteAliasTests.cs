using FluentAssertions;
using Main.Application.Handlers.Producers;
using Main.Entities.Exceptions;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class DeleteAliasTests : IntegrationTest
{
    private ProducerAlias _alias = null!;

    public DeleteAliasTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _alias = await new ProducerAliasBuilder(Faker)
            .WithProducerId(GetContext<ProducerTestContext>().Producers[0].Id)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task DeleteAlias_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteAliasCommand(int.MaxValue, _alias.Alias);
        await Assert.ThrowsAsync<ProducersAliasNotFoundException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteAlias_UnexistingProducerAlias_ThrowsProducersAliasNotFoundException()
    {
        var command =
            new DeleteAliasCommand(_alias.ProducerId, Faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ProducersAliasNotFoundException>(async () =>
            await Mediator.Send(command));
    }

    [Fact]
    public async Task DeleteAlias_Normal_Succeeds()
    {
        var command =
            new DeleteAliasCommand(_alias.ProducerId, _alias.Alias);

        var act = () => Mediator.Send(command);
        await act.Should().NotThrowAsync();

        var dbAliass = await Context.ProducersAliases.AsNoTracking().ToListAsync();
        dbAliass.Should().HaveCount(0);
    }
}