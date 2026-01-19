using Bogus;
using Exceptions.Exceptions.Producers;
using Main.Abstractions.Consts;
using Main.Application.Configs;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Articles;

[Collection("Combined collection")]
public class CreateArticleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly Faker _faker = new("ru");
    private readonly IMediator _mediator;

    public CreateArticleTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        var newProducerModel = MockData.MockData.CreateNewProducerDto(1)[0];
        var command = new CreateProducerCommand(newProducerModel);
        await _mediator.Send(command);
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
        var command = new CreateArticlesCommand(articleList);

        var result = await _mediator.Send(command);

        Assert.Equal(articleList.Count, result.CreatedIds.Count);

        var saved = await _context.Articles.AnyAsync(a => a.ArticleNumber == articleList[0].ArticleNumber);
        Assert.True(saved);
    }

    [Fact]
    public async Task CreateManyArticles_Succeeds()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(Random.Shared.Next(2, 50));
        foreach (var article in articleList)
            article.ProducerId = 1;
        var command = new CreateArticlesCommand(articleList);

        var result = await _mediator.Send(command, CancellationToken.None);

        Assert.Equal(articleList.Count, result.CreatedIds.Count);

        var saved = true;
        foreach (var item in articleList)
        {
            saved = await _context.Articles.AnyAsync(a => a.ArticleNumber == item.ArticleNumber &&
                                                          a.ArticleName == item.Name &&
                                                          a.ProducerId == item.ProducerId);
            if (!saved) break;
        }

        Assert.True(saved);
    }

    [Fact]
    public async Task CreateArticle_WithEmptyList_FailsValidation()
    {
        var command = new CreateArticlesCommand([]);

        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(command));
    }

    [Fact]
    public async Task CreateArticle_WithLongName_FailsValidation()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(1);

        articleList[0].Name = string.Join(" ", _faker.Lorem.Words(100));
        var command = new CreateArticlesCommand(articleList);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _mediator.Send(command));
    }

    [Fact]
    public async Task CreateArticle_WithManyItems_FailsValidation()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(200);
        var command = new CreateArticlesCommand(articleList);
        await Assert.ThrowsAsync<ValidationException>(() =>
            _mediator.Send(command));
    }

    [Fact]
    public async Task CreateArticle_WithInvalidProducer_ThrowsProducerNotFoundException()
    {
        var articleList = MockData.MockData.CreateNewArticleDto(1);
        articleList[0].ProducerId = 9999; // несуществующий ProducerId

        var command = new CreateArticlesCommand(articleList);

        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(command));
        Assert.Equal(ApplicationErrors.ProducersNotFound, exception.Failures[0].ErrorName);
    }
}