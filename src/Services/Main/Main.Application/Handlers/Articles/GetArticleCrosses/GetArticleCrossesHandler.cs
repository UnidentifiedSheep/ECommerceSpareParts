using Application.Common.Interfaces;
using Core.Models;
using Core.StaticFunctions;
using Exceptions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;
using Mapster;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public record GetArticleCrossesQuery<TDto>(int ArticleId, PaginationModel Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetArticleCrossesResult<TDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return string.Format(CacheKeys.ArticleCrossesCacheKey, ArticleId, Pagination.Page,
            Pagination.Size, SortBy);
    }

    public Type GetRelatedType()
    {
        return typeof(ArticleCross);
    }

    public int GetDurationSeconds()
    {
        return 600;
    }
}

public record GetArticleCrossesResult<TDto>(IEnumerable<TDto> Crosses, TDto RequestedArticle);

public class GetArticleCrossesHandler<TDto>(IArticlesRepository articlesRepository, 
    IRelatedDataCollector relatedDataCollector)
    : IQueryHandler<GetArticleCrossesQuery<TDto>, GetArticleCrossesResult<TDto>>
{
    public async Task<GetArticleCrossesResult<TDto>> Handle(GetArticleCrossesQuery<TDto> request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await articlesRepository.GetArticleById(request.ArticleId, false, cancellationToken);
        if (requestedArticle == null) throw new ArticleNotFoundException(request.ArticleId);
        var crosses = await articlesRepository.GetArticleCrosses(request.ArticleId, pagination.Page,
            pagination.Size, request.SortBy, false, cancellationToken);

        var requestedAdapted = requestedArticle.Adapt<TDto>();
        var crossArticlesAdapted = crosses.Adapt<List<TDto>>();

        relatedDataCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        relatedDataCollector.Add(requestedArticle.Id.ToString());

        return new GetArticleCrossesResult<TDto>(crossArticlesAdapted, requestedAdapted);
    }
}