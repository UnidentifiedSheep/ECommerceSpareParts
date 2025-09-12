using Application.Configs;
using Application.Handlers.Articles.CreateArticles;
using Application.Handlers.Articles.SetArticleIndicator;
using Application.Handlers.Producers.CreateProducer;
using Bogus;
using Core.Exceptions.Articles;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Articles;
using static Tests.MockData.MockData;

[Collection("Combined collection")]
public class SetArticleIndicatorTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly Faker _faker = new(Global.Locale);
    
    public SetArticleIndicatorTests(CombinedContainerFixture fixture)
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
    public async Task SetArticleIndicator_WithValidData_Succeeds()
    {
        var article = await _context.Articles.AsNoTracking().FirstOrDefaultAsync();
        
        Assert.NotNull(article);

        var indicator = _faker.Lorem.Word();
        var command = new SetArticleIndicatorCommand(article.Id, indicator);
        await _mediator.Send(command);
        
        var articleAfterSet = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == article.Id);
        Assert.NotNull(articleAfterSet);
        Assert.Equal(indicator, articleAfterSet.Indicator);
    }
    
    [Fact]
    public async Task SetArticleIndicator_WithInvalidArticleId_Fails()
    {
        var indicator = _faker.Lorem.Word();
        var command = new SetArticleIndicatorCommand(999999, indicator);
        await Assert.ThrowsAsync<ArticleNotFoundException>(async () => await _mediator.Send(command));
    }
}