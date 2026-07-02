using FluentAssertions;
using Main.Application.Handlers.Producers.AddOtherName;
using Main.Application.Static;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
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
    public async Task AddOtherProducerName_EmptyProducerName_FailsValidation(string otherName)
    {
        if (otherName == "tooBig") otherName = Faker.Lorem.Letter(200);
        var producer = TestContext.Producers[0];
        var command = new AddOtherNameCommand(
            producer.Id,
            otherName);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new AddOtherNameCommand(
            int.MaxValue,
            Faker.Lorem.Letter(40));
        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducersNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddOtherProducerName_Normal_Succeeds()
    {
        var producer = TestContext.Producers[0];

        var command = new AddOtherNameCommand(
            producer.Id,
            Faker.Lorem.Letter(40));

        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var otherName = await Context.ProducersOtherNames.AsNoTracking().FirstOrDefaultAsync();

        otherName.Should().NotBeNull();

        otherName.ProducerId.Should().Be(producer.Id);
        otherName.Alias.Should().Be(Producer.ToNormalizedName(command.Alias));
    }

    [Fact]
    public async Task AddOtherProducerName_WithSameFields_ThrowsSameProducerOtherNameExists()
    {
        var producer = TestContext.Producers[0];
        var existing = await new ProducerOtherNameBuilder(Faker)
            .WithProducerId(producer.Id)
            .BuildAndAddToDb(Context);

        var command = new AddOtherNameCommand(
            producer.Id,
            existing.Alias);
        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducerOtherNameAlreadyTaken, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddOtherProducerName_WithSameOtherNameForAnotherProducer_ThrowsProducerOtherNameExists()
    {
        var existing = await new ProducerOtherNameBuilder(Faker)
            .WithProducerId(TestContext.Producers[0].Id)
            .BuildAndAddToDb(Context);

        var command = new AddOtherNameCommand(
            TestContext.Producers[1].Id,
            existing.Alias);

        var exception =
            await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));

        Assert.Equal(ApplicationErrors.ProducerOtherNameAlreadyTaken, exception.Failures[0].ErrorName);
    }
}