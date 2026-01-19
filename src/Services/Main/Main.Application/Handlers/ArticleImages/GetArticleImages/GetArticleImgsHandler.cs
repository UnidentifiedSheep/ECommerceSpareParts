using Application.Common.Interfaces;
using Main.Abstractions.Interfaces.DbRepositories;

namespace Main.Application.Handlers.ArticleImages.GetArticleImages;

public record GetArticleImgsQuery(IEnumerable<int> ArticleIds) : IQuery<GetArticleImgsResult>;

public record GetArticleImgsResult(Dictionary<int, HashSet<string>> ArticleImages);

public class GetArticleImgsHandler(IArticleImageRepository imageRepository)
    : IQueryHandler<GetArticleImgsQuery, GetArticleImgsResult>
{
    public async Task<GetArticleImgsResult> Handle(GetArticleImgsQuery request, CancellationToken cancellationToken)
    {
        var imgs = await imageRepository.GetArticlesImages(request.ArticleIds, false, cancellationToken);
        var result = new Dictionary<int, HashSet<string>>();

        foreach (var img in imgs)
            if (!result.TryAdd(img.ArticleId, [img.Path]))
                result[img.ArticleId].Add(img.Path);

        return new GetArticleImgsResult(result);
    }
}