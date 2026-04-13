using Application.Common.Interfaces;
using Main.Abstractions.Constants;
using Main.Abstractions.Models;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public class GetProductCrossesCachePolicy : ICachePolicy<GetProductCrossesQuery>
{
    public string GetCacheKey(GetProductCrossesQuery request) 
        => string.Format(CacheKeys.ProductCrossesCacheKey, request.ArticleId, request.Pagination.Page,
            request.Pagination.Size, request.SortBy);

    public int DurationSeconds => 600;
    public Type? RelatedType => typeof(ArticleCross);
}