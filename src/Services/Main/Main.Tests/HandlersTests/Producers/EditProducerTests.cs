using Abstractions.Models;
using FluentAssertions;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers.EditProducer;
using Main.Entities.Producer;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;

namespace Tests.HandlersTests.Producers;

public class EditProducerTests : IntegrationTest
{
    public EditProducerTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProducerTestContext>();
    }

    private ProducerTestContext TestContext => GetContext<ProducerTestContext>();

    [Theory]
    [InlineData("tooBig")]
    [InlineData("в")]
    public async Task EditProducer_TooLargeName_FailsValidation(string? name)
    {
        if (name == "tooBig") name = Faker.Lorem.Letter(200);
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = name
            }
        };
        var command = new EditProducerCommand(GetFirstProducer().Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_IsSetButNullValue_FailsValidation()
    {
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = null
            }
        };
        var command = new EditProducerCommand(GetFirstProducer().Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_DescriptionTooLarge_FailsValidation()
    {
        var model = new PatchProducerDto
        {
            Description = new PatchField<string?>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(1000)
            }
        };
        var command = new EditProducerCommand(GetFirstProducer().Id, model);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditProducer_NoValuesSet_Succeeds()
    {
        var producer = GetFirstProducer();

        var act = () => Mediator.Send(
            new EditProducerCommand(
                producer.Id,
                new PatchProducerDto()));

        await act.Should().NotThrowAsync();

        var dbProducer = await Context.Producers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == producer.Id);
        dbProducer.Should().NotBeNull();

        dbProducer.Name.Should().Be(producer.Name);
        dbProducer.Description.Should().Be(producer.Description);
    }

    [Fact]
    public async Task EditProducer_Normal_Succeeds()
    {
        var producer = GetFirstProducer();
        var model = new PatchProducerDto
        {
            Name = new PatchField<string>
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(30)
            },
            Description =
            {
                IsSet = true,
                Value = Faker.Lorem.Letter(30)
            }
        };
        var command = new EditProducerCommand(producer.Id, model);
        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();

        var dbProducer = await Context.Producers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == producer.Id);
        dbProducer.Should().NotBeNull();

        dbProducer.Name.Should().Be(Producer.ToNormalizedName(model.Name.Value));
        dbProducer.Description.Should().Be(model.Description.Value);
    }

    private Producer GetFirstProducer()
    {
        return TestContext.Producers[0];
    }
}