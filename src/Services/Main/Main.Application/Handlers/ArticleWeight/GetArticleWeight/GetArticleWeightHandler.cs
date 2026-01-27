using Application.Common.Interfaces;
using Core.StaticFunctions;
using Exceptions.Exceptions.ArticleWeight;
using Main.Abstractions.Dtos.ArticleWeight;
using Main.Abstractions.Interfaces.DbRepositories;
using Mapster;

namespace Main.Application.Handlers.ArticleWeight.GetArticleWeight;

public record GetArticleWeightQuery(int ArticleId) : IQuery<GetArticleWeightResult>, ICacheableQuery
{
    public string GetCacheKey() => string.Format(CacheKeys.ArticleWeightCacheKey, ArticleId);
    public Type? GetRelatedType() => null;
    public int GetDurationSeconds() => 3600;
}

public record GetArticleWeightResult(ArticleWeightDto ArticleWeight);

public class GetArticleWeightHandler(IArticleWeightRepository weightRepository) : IQueryHandler<GetArticleWeightQuery, GetArticleWeightResult>
{
    public async Task<GetArticleWeightResult> Handle(GetArticleWeightQuery request, CancellationToken cancellationToken)
    {
        var weight = await weightRepository.GetArticleWeight(request.ArticleId, false, cancellationToken)
                     ?? throw new ArticleWeightNotFound(request.ArticleId);
        
        return new GetArticleWeightResult(weight.Adapt<ArticleWeightDto>());
    }
}