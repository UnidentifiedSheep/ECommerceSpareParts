using Main.Application.Configs;
using Main.Application.Handlers.Articles.PatchArticle;
using Core.Dtos.Amw.Articles;
using Core.Models;
using Exceptions.Exceptions.Articles;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Articles;

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
        await _mediator.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task EditArticle_NumberAndName_Succeeds()
    {
        var command = new PatchArticleCommand(1,
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
        var command = new PatchArticleCommand(
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
        var command = new PatchArticleCommand(
            1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = "" }
            });

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditArticle_WhenNothingEdited_Succeeds()
    {
        var command = new PatchArticleCommand(
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
        var command = new PatchArticleCommand(
            1,
            new PatchArticleDto
            {
                ArticleNumber = new PatchField<string> { IsSet = true, Value = null }
            });
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
}