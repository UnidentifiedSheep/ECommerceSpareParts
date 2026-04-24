using Main.Application.Handlers.ArticlePairs.CreatePair;
using Main.Application.Handlers.Products.GetProductPair;
using Main.Entities.Exceptions.Products;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.MockData;

namespace Tests.HandlersTests.ArticlePairs;

[Collection("Combined collection")]
public class GetArticlePairTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private int _articleWithOutPair;

    private int _leftArticleId;
    private int _rightArticleId;

    public GetArticlePairTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        var articles = await _context.Products.Take(3).ToListAsync();
        _leftArticleId = articles[0].Id;
        _rightArticleId = articles[1].Id;
        _articleWithOutPair = articles[2].Id;
        await _mediator.Send(new CreatePairCommand(_leftArticleId, _rightArticleId));
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabase();
    }

    [Fact]
    public async Task GetArticlePair_ForLeft_ReturnsRightArticleDto()
    {
        var query = new GetProductPairQuery(_leftArticleId);
        var result = await _mediator.Send(query);
        Assert.Equal(_rightArticleId, result.Pair.Id);
    }

    [Fact]
    public async Task GetArticlePair_ForRight_ReturnsLeftArticleDto()
    {
        var query = new GetProductPairQuery(_rightArticleId);
        var result = await _mediator.Send(query);
        Assert.Equal(_leftArticleId, result.Pair.Id);
    }

    [Fact]
    public async Task GetArticlePair_WhenNoPair_ThrowsNotFound()
    {
        var query = new GetProductPairQuery(_articleWithOutPair);
        await Assert.ThrowsAsync<ArticlePairNotFoundException>(() => _mediator.Send(query));
    }
}