using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using MediatR;
using Search.Abstractions.Dtos;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;

namespace Search.Application.Handler.Articles.AddArticle;

public record AddArticleCommand(ArticleDto Article) : ICommand;

internal class AddArticleHandler(IArticleWriteService articleWriteService) : ICommandHandler<AddArticleCommand>
{
    public Task<Unit> Handle(AddArticleCommand request, CancellationToken cancellationToken)
    {
        var article = request.Article.ToArticle();
        articleWriteService.Add(article);
        return Unit.Task;
    }
}