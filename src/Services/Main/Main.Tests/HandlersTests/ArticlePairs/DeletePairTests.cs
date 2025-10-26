using Main.Application.Handlers.ArticlePairs.CreatePair;
using Main.Application.Handlers.ArticlePairs.DeletePair;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.ArticlePairs;

[Collection("Combined collection")]
public class DeletePairTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private int _leftArticleId;
    private int _rightArticleId;

    public DeletePairTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        var articles = await _context.Articles.Take(2).ToListAsync();
        _leftArticleId = articles[0].Id;
        _rightArticleId = articles[1].Id;
        await _mediator.Send(new CreatePairCommand(_leftArticleId, _rightArticleId));
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeletePair_Success_RemovesBothDirections()
    {
        // ensure created
        var before = await _context.ArticlesPairs.CountAsync(x =>
            (x.ArticleLeft == _leftArticleId && x.ArticleRight == _rightArticleId) ||
            (x.ArticleLeft == _rightArticleId && x.ArticleRight == _leftArticleId));
        Assert.Equal(2, before);

        var cmd = new DeletePairCommand(_leftArticleId);
        var result = await _mediator.Send(cmd);
        Assert.Equal(Unit.Value, result);

        var after = await _context.ArticlesPairs.CountAsync(x =>
            x.ArticleLeft == _leftArticleId || x.ArticleRight == _leftArticleId ||
            x.ArticleLeft == _rightArticleId || x.ArticleRight == _rightArticleId);
        Assert.Equal(0, after);
    }

    [Fact]
    public async Task DeletePair_ForArticleWithoutPairs_DoesNothingAndSucceeds()
    {
        var anyArticleId = await _context.Articles.Select(a => a.Id).FirstAsync();

        var cmd = new DeletePairCommand(anyArticleId);
        var result = await _mediator.Send(cmd);
        Assert.Equal(Unit.Value, result);
        // Still no pairs
        var count = await _context.ArticlesPairs.CountAsync();
        Assert.Equal(0, count);
    }
}
