using Application.Interfaces;
using Core.Dtos.Anonymous.Articles;
using Core.Interfaces.DbRepositories;
using Mapster;

namespace Application.Handlers.ArticleContent.GetArticleContents;

public record GetArticleContentsQuery(int ArticleId) : IQuery<GetArticleContentsResult>;

public record GetArticleContentsResult(IEnumerable<ContentArticleDto> Content);

public class GetArticleContentsHandler(IArticleContentRepository repository)
    : IQueryHandler<GetArticleContentsQuery, GetArticleContentsResult>
{
    public async Task<GetArticleContentsResult> Handle(GetArticleContentsQuery request,
        CancellationToken cancellationToken)
    {
        var content = await repository.GetArticleContents(request.ArticleId, false, cancellationToken);
        var result = content.Adapt<List<ContentArticleDto>>();
        return new GetArticleContentsResult(result);
    }
}