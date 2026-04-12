using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ArticleSizes.GetArticleSizes;

public class GetArticleSizeCachePolicy : ICachePolicy<GetArticleSizeQuery>
{
    public string GetCacheKey(GetArticleSizeQuery request)
        => string.Format(CacheKeys.ArticleSizeCacheKey, request.ArticleId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}