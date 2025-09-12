using Application.Configs;
using Application.Handlers.Producers.AddOtherName;
using Application.Handlers.Producers.CreateProducer;
using Bogus;
using Exceptions.Exceptions.Producers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Producers;

[Collection("Combined collection")]
public class AddOtherNameToProducerTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Locale);
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
        var newProducerModel = CreateNewProducerDto(1)[0];
        var command = new CreateProducerCommand(newProducerModel);
        await _mediator.Send(command);
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
        var command = new AddOtherNameCommand(producer.Id, " ", null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_TooLargeName_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var command = new AddOtherNameCommand(producer.Id, _faker.Lorem.Letter(200), null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_TooLargeUsage_FailsValidation()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);
        var command = new AddOtherNameCommand(producer.Id, _faker.Lorem.Letter(40), _faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new AddOtherNameCommand(int.MaxValue, _faker.Lorem.Letter(40), _faker.Lorem.Letter(10));
        await Assert.ThrowsAsync<ProducerNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddOtherProducerName_Normal_Succeeds()
    {
        var producer = await _context.Producers.AsNoTracking().FirstOrDefaultAsync();
        Assert.NotNull(producer);

        var otherName = _faker.Lorem.Letter(40);
        var usage = _faker.Lorem.Letter(10);
        var command = new AddOtherNameCommand(producer.Id, otherName, usage);
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
        await _mediator.Send(new AddOtherNameCommand(producer.Id, otherName, usage));

        var command = new AddOtherNameCommand(producer.Id, otherName, usage);
        await Assert.ThrowsAsync<SameProducerOtherNameExistsException>(async () => await _mediator.Send(command));
    }
}