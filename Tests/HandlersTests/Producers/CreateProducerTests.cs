using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Producers.CreateProducer;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Producers;

[Collection("Combined collection")]
public class CreateProducerTests : IAsyncLifetime
{
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public CreateProducerTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
    
    [Fact]
    public async Task CreateProducer_TooShortName_FailsValidation()
    {
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        newProducerModel.ProducerName = "a";
        var command = new CreateProducerCommand(newProducerModel);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateProducer_EmptyName_FailsValidation()
    {
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        newProducerModel.ProducerName = "   ";
        var command = new CreateProducerCommand(newProducerModel);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateProducer_TooLargeDescription_FailsValidation()
    {
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        newProducerModel.Description = _faker.Lorem.Letter(600);
        var command = new CreateProducerCommand(newProducerModel);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateProducer_TooLargeDescription_Succeeds()
    {
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        var command = new CreateProducerCommand(newProducerModel);
        var result = await _mediator.Send(command);
        
        var createdProducer = await _context.Producers
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == result.ProducerId);
        Assert.NotNull(createdProducer);
        Assert.Equal(createdProducer.Description, newProducerModel.Description);
        Assert.Equal(createdProducer.Name, newProducerModel.ProducerName);
        Assert.Equal(createdProducer.IsOe, newProducerModel.IsOe);
    }
}