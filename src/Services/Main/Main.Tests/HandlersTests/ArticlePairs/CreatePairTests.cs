using FluentValidation;
using Main.Application.Handlers.ArticlePairs.CreatePair;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.ArticlePairs;

[Collection("Combined collection")]
public class CreatePairTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    private int _leftArticleId;
    private int _rightArticleId;

    public CreatePairTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        var articles = await _context.Articles.Take(2).ToListAsync();
        _leftArticleId = articles[0].Id;
        _rightArticleId = articles[1].Id;
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreatePair_Success_CreatesBidirectionalRows()
    {
        var cmd = new CreatePairCommand(_leftArticleId, _rightArticleId);
        var result = await _mediator.Send(cmd);
        Assert.Equal(Unit.Value, result);

        // Two rows must be created: (left,right) and (right,left)
        var pairs = await _context.ArticlesPairs
            .Where(p => (p.ArticleLeft == _leftArticleId && p.ArticleRight == _rightArticleId)
                        || (p.ArticleLeft == _rightArticleId && p.ArticleRight == _leftArticleId))
            .ToListAsync();
        Assert.Equal(2, pairs.Count);
    }

    [Fact]
    public async Task CreatePair_SameArticle_ThrowsValidation()
    {
        var cmd = new CreatePairCommand(_leftArticleId, _leftArticleId);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreatePair_Twice_ViolatesUniqueLeftConstraint()
    {
        var cmd = new CreatePairCommand(_leftArticleId, _rightArticleId);
        await _mediator.Send(cmd);
        
        using var scope = _serviceProvider.CreateScope();
        var otherMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        await Assert.ThrowsAnyAsync<DbUpdateException>(async () =>
        {
            await otherMediator.Send(cmd);
        });
    }
}
