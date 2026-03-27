using Mediator;
using Search.Abstractions.Dtos;
using Search.Abstractions.Exceptions.Articles;
using Search.Abstractions.Interfaces.Persistence;
using Search.Application.Configs;

namespace Search.Application.Handler.Articles.GetArticle;

public record GetArticleQuery(int ArticleId) : IQuery<GetArticleResult>;

public record GetArticleResult(ArticleDto Article);

internal class GetArticleHandler(IArticleReadService readService) : IQueryHandler<GetArticleQuery, GetArticleResult>
{
    public ValueTask<GetArticleResult> Handle(GetArticleQuery request, CancellationToken cancellationToken)
    {
        var article = readService.GetArticle(request.ArticleId) ??
                      throw new ArticleNotFoundException(request.ArticleId);

        var result = article.ToDto();
        return ValueTask.FromResult(new GetArticleResult(result));
    }
}