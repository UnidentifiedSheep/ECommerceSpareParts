using FluentAssertions;
using Main.Application.Dtos.Producer;
using Main.Application.Handlers.Producers.CreateProducer;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Producers;

public class CreateProducerTests(CombinedContainerFixture fixture) : TestBase(fixture)
{
    [Theory]
    [InlineData("")]
    [InlineData("           ")]
    [InlineData("a")]
    public async Task CreateProducer_TooShortName_FailsValidation(string name)
    {
        var producer = CreateDto() with { Name = name };
        var command = new CreateProducerCommand(producer);
        
        var act = () => Mediator.Send(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProducer_TooLargeDescription_FailsValidation()
    {
        var producer = CreateDto() with { Description = Faker.Lorem.Letter(600) };
        var command = new CreateProducerCommand(producer);
        
        var act = () => Mediator.Send(command);
        
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProducer_WithValidData_Succeeds()
    {
        var producer = CreateDto();
        var command = new CreateProducerCommand(producer);
        
        var act = () => Mediator.Send(command);

        await act.Should().NotThrowAsync();
        
        var createdProducer = await Context.Producers.AsNoTracking().FirstOrDefaultAsync();
        
        createdProducer.Should().NotBeNull();
        
        createdProducer.Name.Should().Be(producer.Name);
        createdProducer.Description.Should().Be(producer.Description);
    }

    private NewProducerDto CreateDto()
        => new()
        {
            Name = Faker.Lorem.Word(),
            Description = Faker.Lorem.Sentence()
        };
}