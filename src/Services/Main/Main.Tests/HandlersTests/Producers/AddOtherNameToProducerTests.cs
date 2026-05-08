using FluentAssertions;
using Main.Abstractions.Constants;
using Main.Application.Handlers.Producers.AddOtherName;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders;
using Tests.TestContexts;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Producers;

public class AddOtherNameToProducerTests : IntegrationTest
{
    public AddOtherNameToProducerTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Theory]
    [InlineData(" ")]
    [InlineData("tooBig")]
    public async Task AddOtherProducerName_EmptyProducerName_FailsValidation(string otherName)
    {
        if (otherName == "tooBig")
            otherName = Faker.Lorem.Letter(200);
        var producer = TestContext.Producers[0];
        var command = new AddOtherNameCommand(producer.Id, otherName, "null");
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_TooLargeUsage_FailsValidation()
    {
        var producer = TestContext.Producers[0];
        var command = new AddOtherNameCommand(
            producer.Id,
            Faker.Lorem.Letter(40),
            Faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new AddOtherNameCommand(
            int.MaxValue,
            Faker.Lorem.Letter(40),
            Faker.Lorem.Letter(10));
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducersNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddOtherProducerName_Normal_Succeeds()
    {
        var producer = TestContext.Producers[0];

        var command = new AddOtherNameCommand(
            producer.Id,
            Faker.Lorem.Letter(40),
            Faker.Lorem.Letter(10));

        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var otherName = await Context.ProducersOtherNames.AsNoTracking().FirstOrDefaultAsync();

        otherName.Should().NotBeNull();

        otherName.ProducerId.Should().Be(producer.Id);
        otherName.OtherName.Should().Be(command.OtherName);
        otherName.WhereUsed.Should().Be(command.WhereUsed);
    }

    [Fact]
    public async Task AddOtherProducerName_WithSameFields_ThrowsSameProducerOtherNameExists()
    {
        var producer = TestContext.Producers[0];
        var existing = await new ProducerOtherNameBuilder(Faker)
            .WithProducerId(producer.Id)
            .BuildAndAddToDb(Context);

        var command = new AddOtherNameCommand(producer.Id, existing.OtherName, existing.WhereUsed);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducerOtherNameAlreadyTaken, exception.Failures[0].ErrorName);
    }
}