using Core.Interface;
using Microsoft.EntityFrameworkCore;
using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleImgs;

public record GetArticleImgsQuery(IEnumerable<int> ArticleIds) : IQuery<GetArticleImgsResult>;
public record GetArticleImgsResult(Dictionary<int, HashSet<string>> ArticleImages);

public class GetArticleImgsHandler(DContext context) : IQueryHandler<GetArticleImgsQuery, GetArticleImgsResult>
{
    public async Task<GetArticleImgsResult> Handle(GetArticleImgsQuery request, CancellationToken cancellationToken)
    {
        var imgs = await context.ArticleImages
            .AsNoTracking()
            .Where(x => request.ArticleIds.Contains(x.ArticleId))
            .ToListAsync(cancellationToken: cancellationToken);
        var result = new Dictionary<int, HashSet<string>>();
        foreach (var img in imgs)
        {
            var added = result.TryAdd(img.ArticleId, [img.Path]);
            if (!added) result[img.ArticleId].Add(img.Path);
        }
        return new GetArticleImgsResult(result);
    }
}