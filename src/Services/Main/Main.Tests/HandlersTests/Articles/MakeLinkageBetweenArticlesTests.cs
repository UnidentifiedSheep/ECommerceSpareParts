using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;
using Main.Application.Handlers.Producers.CreateProducer;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;

namespace Tests.HandlersTests.Articles;

[Collection("Combined collection")]
public class MakeLinkageBetweenArticlesTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    public MakeLinkageBetweenArticlesTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        var newProducerModel = CreateNewProducerDto(1)[0];
        var producerCommand = new CreateProducerCommand(newProducerModel);
        var producerId = (await _mediator.Send(producerCommand)).ProducerId;

        var articleList = CreateNewArticleDto(10);
        foreach (var article in articleList)
            article.ProducerId = producerId;

        var articleCommand = new CreateArticlesCommand(articleList);
        await _mediator.Send(articleCommand);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task MakeLinkage_SameIds_FailsValidation()
    {
        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 1,
            LinkageType = ArticleLinkageType.FullCross
        };
        var command = new MakeLinkageBetweenArticlesCommand([newLinkage]);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task MakeLinkage_SingleCrosses_Succeeds()
    {
        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageType.SingleCross
        };
        var command = new MakeLinkageBetweenArticlesCommand([newLinkage]);

        var result = await _mediator.Send(command);
        Assert.Equal(Unit.Value, result);

        var crosses = await _context.Articles
            .Where(x => (x.Id == 1 && x.ArticleCrosses.FirstOrDefault(z => z.Id == 2) != null) ||
                        (x.Id == 2 && x.ArticleCrosses.FirstOrDefault(z => z.Id == 1) != null))
            .CountAsync();

        Assert.Equal(2, crosses);
    }

    [Fact]
    public async Task MakeLinkage_FullCrosses_Succeeds()
    {
        await ClearCrosses(_context);
        await _context.AddArticleCross(1, 3);
        await _context.AddArticleCross(2, 4);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageType.FullCross
        };
        var command = new MakeLinkageBetweenArticlesCommand([newLinkage]);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int ArticleId, int ArticleCrossId)>
        {
            (1, 2), (1, 4), (3, 2), (3, 4),
            (2, 1), (4, 1), (2, 3), (4, 3)
        };

        var actualLinks = await GetArticleCrosses(_context);

        var matchingLinks = actualLinks
            .Where(x => expectedLinks.Contains((x.ArticleId, x.ArticleCrossId)))
            .ToList();

        Assert.Equal(expectedLinks.Count, matchingLinks.Count);

        var missing = expectedLinks.Except(matchingLinks.Select(x => (x.ArticleId, x.ArticleCrossId))).ToList();
        Assert.True(missing.Count == 0, $"Missing expected pairs: {string.Join(", ", missing)}");
    }

    [Fact]
    public async Task MakeLinkage_FullRightToLeft_Succeeds()
    {
        await ClearCrosses(_context);
        await _context.AddArticleCross(2, 4);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageType.FullRightToLeftCross
        };
        var command = new MakeLinkageBetweenArticlesCommand([newLinkage]);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int, int)>
        {
            (1, 2), (2, 1), (1, 4), (4, 1)
        };

        var actualLinks = await GetArticleCrosses(_context);

        var matchingLinks = actualLinks
            .Where(x => expectedLinks.Contains((x.ArticleId, x.ArticleCrossId)))
            .ToList();

        Assert.Equal(expectedLinks.Count, matchingLinks.Count);
        var missing = expectedLinks.Except(matchingLinks.Select(x => (x.ArticleId, x.ArticleCrossId))).ToList();
        Assert.True(missing.Count == 0, $"Missing expected pairs: {string.Join(", ", missing)}");
    }

    [Fact]
    public async Task MakeLinkage_FullLeftToRightCross_Succeeds()
    {
        await ClearCrosses(_context);
        await _context.AddArticleCross(1, 3);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageType.FullLeftToRightCross
        };
        var command = new MakeLinkageBetweenArticlesCommand([newLinkage]);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int, int)>
        {
            (1, 2), (2, 1), (3, 2), (2, 3)
        };

        var actualLinks = await GetArticleCrosses(_context);

        var matchingLinks = actualLinks
            .Where(x => expectedLinks.Contains((x.ArticleId, x.ArticleCrossId)))
            .ToList();

        Assert.Equal(expectedLinks.Count, matchingLinks.Count);
        var missing = expectedLinks.Except(matchingLinks.Select(x => (x.ArticleId, x.ArticleCrossId))).ToList();
        Assert.True(missing.Count == 0, $"Missing expected pairs: {string.Join(", ", missing)}");
    }

    private async Task ClearCrosses(DContext context)
    {
        var articles = await context.Articles.Include(a => a.ArticleCrosses).ToListAsync();
        articles.ForEach(a => a.ArticleCrosses.Clear());
        await context.SaveChangesAsync();
    }

    public async Task<List<(int ArticleId, int ArticleCrossId)>> GetArticleCrosses(DContext context)
    {
        var articles = await context.Articles
            .Include(a => a.ArticleCrosses)
            .ToDictionaryAsync(x => x.Id, z => z.ArticleCrosses);
        var crosses = articles.Select(x => x.Value.Select(y => (x.Key, y.Id)));
        return crosses.SelectMany(x => x).ToList();
    }
    
}