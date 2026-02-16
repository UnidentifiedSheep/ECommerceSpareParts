using Mediator;
using Search.Abstractions.Dtos;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;
using Search.Entities;

namespace Search.Application.Handler.Articles.AddArticle;

public record AddArticleCommand(ArticleDto Article) : ICommand;

internal class AddArticleHandler(IArticleRepository articleRepository) : ICommandHandler<AddArticleCommand>
{
    public ValueTask<Unit> Handle(AddArticleCommand request, CancellationToken cancellationToken)
    {
        Article article = request.Article.ToArticle();
        articleRepository.Add(article);
        return Unit.ValueTask;
    }
}