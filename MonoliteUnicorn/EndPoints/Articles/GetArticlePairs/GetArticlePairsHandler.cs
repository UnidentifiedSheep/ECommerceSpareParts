using Core.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.Dtos.Anonymous.Articles;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticlePairs;

public record GetArticlePairsQuery(int ArticleId) : IQuery<GetArticlePairsResult>;
public record GetArticlePairsResult(IEnumerable<ArticleDto> Pairs); 
public class GetArticlePairsHandler(DContext context) : IQueryHandler<GetArticlePairsQuery, GetArticlePairsResult>
{
    public async Task<GetArticlePairsResult> Handle(GetArticlePairsQuery request, CancellationToken cancellationToken)
    {
        var articles = await context.Articles
            .FromSql($"SELECT DISTINCT a.id AS article_id, a.* FROM ( SELECT article_left, article_right FROM articles_pair WHERE article_left = {request.ArticleId} OR article_right = {request.ArticleId}) AS t JOIN articles AS a ON a.id = t.article_left OR a.id = t.article_right")
            .AsNoTracking()
            .Include(x => x.Producer)
            .ToListAsync(cancellationToken: cancellationToken);
        articles.RemoveAll(x => x.Id == request.ArticleId);
        return new GetArticlePairsResult(articles.Adapt<List<ArticleDto>>());
    }
}