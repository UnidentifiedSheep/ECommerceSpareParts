using Core.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.EndPoints.Articles.EditArticle;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.Articles;

[Collection("Combined collection")]
public class EditArticleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public EditArticleTests(CombinedContainerFixture fixture)
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
    public async Task EditArticle_NumberAndName_Succeeds()
    {
        var command = new EditArticleCommand(1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = "67890" },
                ArticleName = new PatchField<string> { IsSet = true, Value = "Updated Article" }
            });

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);

        var updatedArticle = await _context.Articles.FindAsync(1);
        Assert.Equal("67890", updatedArticle!.ArticleNumber);
        Assert.Equal("Updated Article", updatedArticle.ArticleName);
    }
    
    [Fact]
    public async Task EditArticle_WithInvalidArticleId_FailsValidation()
    {
        var command = new EditArticleCommand(
            999,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = "67890" }
            });

        await Assert.ThrowsAsync<ArticleNotFoundException>(async () =>
            await _mediator.Send(command));
    }

    [Fact]
    public async Task EditArticle_WithEmptyArticleNumber_FailsValidation()
    {
        var command = new EditArticleCommand(
            1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = "" }
            });

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditArticle_WhenNothingEdited_Succeeds()
    {
        var command = new EditArticleCommand(
            1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = false, Value = null }
            });

        var result = await _mediator.Send(command);

        Assert.Equal(Unit.Value, result);
    }
        
    [Fact]
    public async Task EditArticle_WhenArticleNumberNull_FailsValidation()
    {
        var command = new EditArticleCommand(
            1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = null }
            });
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
}