using Application.Configs;
using Application.Handlers.Producers.DeleteProducer;
using Exceptions.Exceptions.Producers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Producers;

[Collection("Combined collection")]
public class DeleteProducerTest : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public DeleteProducerTest(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
        
    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
    
    [Fact]
    public async Task DeleteProducer_InvalidProducerId_ThrowsProducerNotFound()
    {
        var command = new DeleteProducerCommand(int.MaxValue);
        await Assert.ThrowsAsync<ProducerNotFoundException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task DeleteProducer_WhenProducerHasArticle_ThrowsCannotDeleteProducerWithArticles()
    {
        var article = await _context.Articles.FirstOrDefaultAsync();
        Assert.NotNull(article);
        var command = new DeleteProducerCommand(article.ProducerId);
        await Assert.ThrowsAsync<CannotDeleteProducerWithArticlesException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task DeleteProducer_Normal_Succeeds()
    {
        var producer = await _context.Producers
            .Include(x => x.Articles)
            .FirstOrDefaultAsync();
        Assert.NotNull(producer);
        
        _context.Articles.RemoveRange(producer.Articles);
        await _context.SaveChangesAsync();
        
        var command = new DeleteProducerCommand(producer.Id);
        await _mediator.Send(command);
    }
}