using Application.Common.Interfaces;
using Core.StaticFunctions;
using Exceptions.Exceptions.ArticleSizes;
using Main.Abstractions.Dtos.ArticleSizes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.ArticleSizes.GetArticleSizes;

public record GetArticleSizeQuery(int ArticleId) : IQuery<GetArticleSizeResult>, ICacheableQuery
{
    public string GetCacheKey() => string.Format(CacheKeys.ArticleSizeCacheKey, ArticleId);
    public Type? GetRelatedType() => null;
    public int GetDurationSeconds() => 3600;
}

public record GetArticleSizeResult(ArticleSizeDto ArticleSize);

public class GetArticleSizeHandler(IArticleSizesRepository sizesRepository) 
    : IQueryHandler<GetArticleSizeQuery, GetArticleSizeResult>
{
    public async Task<GetArticleSizeResult> Handle(GetArticleSizeQuery request, CancellationToken cancellationToken)
    {
        var size = await sizesRepository.GetArticleSizes(request.ArticleId, false, cancellationToken)
                   ?? throw new ArticleSizesNotFoundException(request.ArticleId);
        return new GetArticleSizeResult(size.Adapt<ArticleSizeDto>());
    }
}