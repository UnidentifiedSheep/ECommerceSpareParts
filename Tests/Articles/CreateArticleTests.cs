using Bogus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Articles.CreateArticle;
using MonoliteUnicorn.Exceptions.Producers;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.Articles;

[Collection("Combined collection")]
public class CreateArticleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly Faker _faker = new("ru");
    public CreateArticleTests(CombinedContainerFixture fixture)
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
    public async Task CreateOneArticle_Succeeds()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(1);
        
        articleList[0].ProducerId = 1;

        var command = new CreateArticleCommand(articleList);

        var result = await _mediator.Send(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        
        var saved = await _context.Articles.AnyAsync(a => a.ArticleNumber == articleList[0].ArticleNumber);
        Assert.True(saved);
    }
    
    [Fact]
    public async Task CreateManyArticles_Succeeds()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(Random.Shared.Next(2, 50));

        var command = new CreateArticleCommand(articleList);

        var result = await _mediator.Send(command, CancellationToken.None);

        Assert.Equal(Unit.Value, result);

        var saved = true;
        foreach (var item in articleList)
        {
            saved = await _context.Articles.AnyAsync(a => a.ArticleNumber == item.ArticleNumber && 
                                                          a.ArticleName == item.Name && a.ProducerId == item.ProducerId);
            if (!saved) break;
        }
        
        Assert.True(saved);
    }
    
    [Fact]
    public async Task CreateArticle_WithEmptyList_FailsValidation()
    {
        var command = new CreateArticleCommand([]);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateArticle_WithLongName_FailsValidation()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(1);
        
        articleList[0].Name = string.Join(" ", _faker.Lorem.Words(100));
        var command = new CreateArticleCommand(articleList);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => 
            _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateArticle_WithManyItems_FailsValidation()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(200);
        var command = new CreateArticleCommand(articleList);
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => 
            _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateArticle_WithInvalidProducer_ThrowsProducerNotFoundException()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(1);
        articleList[0].ProducerId = 9999; // несуществующий ProducerId

        var command = new CreateArticleCommand(articleList);

        await Assert.ThrowsAsync<ProducerNotFoundException>(() => _mediator.Send(command));
    }
}