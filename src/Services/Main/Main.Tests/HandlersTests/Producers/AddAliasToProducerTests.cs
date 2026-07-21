using FluentAssertions;
using Main.Application.Handlers.ProducerAliases.AddAlias;
using Main.Application.Static;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class AddAliasToProducerTests : IntegrationTest
{
    public AddAliasToProducerTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Theory]
    [InlineData(" ")]
    [InlineData("tooBig")]
    public async Task AddProducerAlias_EmptyProducerName_FailsValidation(string alias)
    {
        if (alias == "tooBig") alias = Faker.Lorem.Letter(200);
        var producer = TestContext.Producers[0];
        var command = new AddAliasCommand(
            producer.Id,
            alias);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddProducerAlias_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new AddAliasCommand(
            int.MaxValue,
            Faker.Lorem.Letter(40));
        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducersNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddProducerAlias_Normal_Succeeds()
    {
        var producer = TestContext.Producers[0];

        var command = new AddAliasCommand(
            producer.Id,
            Faker.Lorem.Letter(40));

        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var alias = await Context.ProducersAliases.AsNoTracking().FirstOrDefaultAsync();

        alias.Should().NotBeNull();

        alias.ProducerId.Should().Be(producer.Id);
        alias.Alias.Should().Be(Producer.ToNormalizedName(command.Alias));
    }

    [Fact]
    public async Task AddProducerAlias_WithSameFields_ThrowsSameProducerAliasExists()
    {
        var producer = TestContext.Producers[0];
        var existing = await new ProducerAliasBuilder(Faker)
            .WithProducerId(producer.Id)
            .BuildAndAddToDb(Context);

        var command = new AddAliasCommand(
            producer.Id,
            existing.Alias);
        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducerAliasAlreadyTaken, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddProducerAlias_WithSameAliasForAnotherProducer_ThrowsProducerAliasExists()
    {
        var existing = await new ProducerAliasBuilder(Faker)
            .WithProducerId(TestContext.Producers[0].Id)
            .BuildAndAddToDb(Context);

        var command = new AddAliasCommand(
            TestContext.Producers[1].Id,
            existing.Alias);

        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));

        Assert.Equal(ApplicationErrors.ProducerAliasAlreadyTaken, exception.Failures[0].ErrorName);
    }
}