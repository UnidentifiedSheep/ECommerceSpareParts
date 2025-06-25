using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Producers.AddOtherNamesToProducer;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;

namespace Tests.Producers;

[Collection("Combined collection")]
public class AddOtherNameToProducerTests : IAsyncLifetime
{
    private readonly Faker _faker = new(Locale);
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public AddOtherNameToProducerTests(CombinedContainerFixture fixture)
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
    public async Task AddOtherProducerName_EmptyProducerName_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var command = new AddOtherNameToProducerCommand(producer.Id, " ", null);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddOtherProducerName_TooLargeName_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var command = new AddOtherNameToProducerCommand(producer.Id, _faker.Lorem.Letter(200), null);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddOtherProducerName_TooLargeUsage_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var command = new AddOtherNameToProducerCommand(producer.Id, _faker.Lorem.Letter(40), _faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddOtherProducerName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new AddOtherNameToProducerCommand(int.MaxValue, _faker.Lorem.Letter(40), _faker.Lorem.Letter(10));
        await Assert.ThrowsAsync<ProducerNotFoundException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddOtherProducerName_Normal_Succeeds()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);

        var otherName = _faker.Lorem.Letter(40);
        var usage = _faker.Lorem.Letter(10);
        var command = new AddOtherNameToProducerCommand(producer.Id, otherName, usage);
        await _mediator.Send(command);
        
        var producerOtherName = await _context.ProducersOtherNames
            .AsNoTracking()
            .Where(x => x.ProducerId == producer.Id &&
                        x.ProducerOtherName == otherName && x.WhereUsed == usage)
            .FirstOrDefaultAsync();
        Assert.NotNull(producerOtherName);
    }
    
    [Fact]
    public async Task AddOtherProducerName_WithSameFields_ThrowsSameProducerOtherNameExists()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var otherName = _faker.Lorem.Letter(40);
        var usage = _faker.Lorem.Letter(10);
        await _mediator.Send(new AddOtherNameToProducerCommand(producer.Id, otherName, usage));
        
        var command = new AddOtherNameToProducerCommand(producer.Id, otherName, usage);
        await Assert.ThrowsAsync<SameProducerOtherNameExistsException>(async () => await _mediator.Send(command));
    }
}