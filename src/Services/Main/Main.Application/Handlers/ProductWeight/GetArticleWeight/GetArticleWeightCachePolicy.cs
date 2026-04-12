using Application.Common.Interfaces;
using Main.Abstractions.Constants;

namespace Main.Application.Handlers.ArticleWeight.GetArticleWeight;

public class GetArticleWeightCachePolicy : ICachePolicy<GetArticleWeightQuery>
{
    public string GetCacheKey(GetArticleWeightQuery request)
        => string.Format(CacheKeys.ArticleWeightCacheKey, request.ArticleId);

    public int DurationSeconds => 3600;
    public Type? RelatedType => null;
}