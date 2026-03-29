using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Extensions;
using Main.Abstractions.Constants;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Models;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public record GetArticleCrossesQuery<TDto>(int ArticleId, PaginationModel Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetArticleCrossesResult<TDto>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        return string.Format(CacheKeys.ArticleCrossesCacheKey, typeof(TDto).Name, ArticleId, Pagination.Page,
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

public class GetArticleCrossesHandler<TDto>(
    IArticlesRepository articlesRepository,
    IRelatedDataCollector relatedDataCollector)
    : IQueryHandler<GetArticleCrossesQuery<TDto>, GetArticleCrossesResult<TDto>>
{
    public async Task<GetArticleCrossesResult<TDto>> Handle(
        GetArticleCrossesQuery<TDto> request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await GetRequestedArticle(request.ArticleId, cancellationToken);
        
        var crosses = await GetCrosses(request.ArticleId, pagination, request.SortBy, cancellationToken);

        var requestedAdapted = requestedArticle.Adapt<TDto>();
        var crossArticlesAdapted = crosses.Adapt<List<TDto>>();

        relatedDataCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        relatedDataCollector.Add(requestedArticle.Id.ToString());

        return new GetArticleCrossesResult<TDto>(crossArticlesAdapted, requestedAdapted);
    }

    private async Task<Article> GetRequestedArticle(int id, CancellationToken token)
    {
        var queryOptions = new QueryOptions<Article, int>()
        {
            Data = id
        }.WithTracking(false)
        .WithInclude(x => x.Producer);
        var requestedArticle = await articlesRepository.GetArticleById(queryOptions, token);
        if (requestedArticle == null) throw new ArticleNotFoundException(id);
        return requestedArticle;
    }

    private async Task<IReadOnlyList<Article>> GetCrosses(
        int articleId, 
        PaginationModel pagination,
        string? sortBy,
        CancellationToken token)
    {
        var queryOptions = new QueryOptions<Article, int>
            {
                Data = articleId
            }.WithPage(pagination.Page)
            .WithSize(pagination.Size)
            .WithTracking(false)
            .WithInclude(x => x.Producer)
            .WithSorting(sortBy);
        
        return await articlesRepository.GetArticleCrosses(queryOptions, token);
    }
}