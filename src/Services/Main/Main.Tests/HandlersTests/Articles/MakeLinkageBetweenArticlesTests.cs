using Core.Dtos.Amw.Articles;
using Core.Enums;
using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.Articles.CreateArticles;
using Main.Application.Handlers.Articles.MakeLinkageBetweenArticles;
using Main.Application.Handlers.Producers.CreateProducer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
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
            LinkageType = ArticleLinkageTypes.FullCross
        };
        var command = new MakeLinkageBetweenArticlesCommand(newLinkage);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task MakeLinkage_SingleCrosses_Succeeds()
    {
        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageTypes.SingleCross
        };
        var command = new MakeLinkageBetweenArticlesCommand(newLinkage);

        var result = await _mediator.Send(command);
        Assert.Equal(Unit.Value, result);

        var crosses = await _context.ArticleCrosses
            .Where(x => (x.ArticleId == 1 && x.ArticleCrossId == 2) || (x.ArticleId == 2 && x.ArticleCrossId == 1))
            .CountAsync();

        Assert.Equal(2, crosses);
    }

    [Fact]
    public async Task MakeLinkage_FullCrosses_Succeeds()
    {
        _context.ArticleCrosses.RemoveRange(await _context.ArticleCrosses.ToListAsync());
        await _context.AddArticleCross(1, 3);
        await _context.AddArticleCross(2, 4);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageTypes.FullCross
        };
        var command = new MakeLinkageBetweenArticlesCommand(newLinkage);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int ArticleId, int ArticleCrossId)>
        {
            (1, 2), (1, 4), (3, 2), (3, 4),
            (2, 1), (4, 1), (2, 3), (4, 3)
        };

        var actualLinks = await _context.ArticleCrosses
            .AsNoTracking()
            .ToListAsync();

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
        _context.ArticleCrosses.RemoveRange(await _context.ArticleCrosses.ToListAsync());
        await _context.AddArticleCross(2, 4);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageTypes.FullRightToLeftCross
        };
        var command = new MakeLinkageBetweenArticlesCommand(newLinkage);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int, int)>
        {
            (1, 2), (2, 1), (1, 4), (4, 1)
        };

        var actualLinks = await _context.ArticleCrosses
            .AsNoTracking()
            .ToListAsync();

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
        _context.ArticleCrosses.RemoveRange(await _context.ArticleCrosses.ToListAsync());
        await _context.AddArticleCross(1, 3);

        var newLinkage = new NewArticleLinkageDto
        {
            ArticleId = 1,
            CrossArticleId = 2,
            LinkageType = ArticleLinkageTypes.FullLeftToRightCross
        };
        var command = new MakeLinkageBetweenArticlesCommand(newLinkage);

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var expectedLinks = new HashSet<(int, int)>
        {
            (1, 2), (2, 1), (3, 2), (2, 3)
        };

        var actualLinks = await _context.ArticleCrosses
            .AsNoTracking()
            .ToListAsync();

        var matchingLinks = actualLinks
            .Where(x => expectedLinks.Contains((x.ArticleId, x.ArticleCrossId)))
            .ToList();

        Assert.Equal(expectedLinks.Count, matchingLinks.Count);
        var missing = expectedLinks.Except(matchingLinks.Select(x => (x.ArticleId, x.ArticleCrossId))).ToList();
        Assert.True(missing.Count == 0, $"Missing expected pairs: {string.Join(", ", missing)}");
    }
}