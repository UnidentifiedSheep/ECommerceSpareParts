using Application.Configs;
using Application.Handlers.Producers.DeleteOtherName;
using Bogus;
using Core.Entities;
using Exceptions.Exceptions.Producers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Producers;

[Collection("Combined collection")]
public class DeleteOtherNameTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new(Global.Locale);
    private readonly IMediator _mediator;
    private ProducersOtherName _otherName = null!;

    public DeleteOtherNameTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();

        var producer = await _context.Producers.AsNoTracking().FirstAsync();
        var otherName = _faker.Lorem.Letter(40);
        var usage = _faker.Lorem.Letter(10);
        await _context.Database
            .ExecuteSqlAsync($"""
                              insert into producers_other_names (producer_id, producer_other_name, where_used) 
                              values ({producer.Id}, {otherName}, {usage});
                              """);
        _otherName = await _context.ProducersOtherNames
            .FirstAsync(x => x.ProducerId == 1 &&
                             x.ProducerOtherName == otherName &&
                             x.WhereUsed == usage);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteOtherName_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteOtherNameCommand(int.MaxValue, _otherName.ProducerOtherName, _otherName.WhereUsed);
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_UnexistingProducerOtherName_ThrowsProducersOtherNameNotFoundException()
    {
        var command =
            new DeleteOtherNameCommand(_otherName.ProducerId, _faker.Lorem.Letter(200), _faker.Lorem.Letter(200));
        await Assert.ThrowsAsync<ProducersOtherNameNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteOtherName_Normal_Succeeds()
    {
        var command =
            new DeleteOtherNameCommand(_otherName.ProducerId, _otherName.ProducerOtherName, _otherName.WhereUsed);
        await _mediator.Send(command);
        var otherName = await _context.ProducersOtherNames
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProducerId == _otherName.ProducerId &&
                                      x.WhereUsed == _otherName.WhereUsed &&
                                      x.ProducerOtherName == _otherName.ProducerOtherName);
        Assert.Null(otherName);
    }
}