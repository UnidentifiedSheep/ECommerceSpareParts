using Abstractions.Models;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Extensions;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public record GetArticleCrossesQuery(int ArticleId, PaginationModel Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetArticleCrossesResult>;

public record GetArticleCrossesResult(IEnumerable<ArticleFullDto> Crosses, ArticleFullDto RequestedArticle);

public class GetArticleCrossesHandler(
    IArticlesRepository articlesRepository,
    IRelatedDataCollector relatedDataCollector)
    : IQueryHandler<GetArticleCrossesQuery, GetArticleCrossesResult>
{
    public async Task<GetArticleCrossesResult> Handle(
        GetArticleCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await GetRequestedArticle(request.ArticleId, cancellationToken);
        
        var crosses = await GetCrosses(request.ArticleId, pagination, request.SortBy, cancellationToken);

        var requestedAdapted = requestedArticle.Adapt<ArticleFullDto>();
        var crossArticlesAdapted = crosses.Adapt<List<ArticleFullDto>>();

        relatedDataCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        relatedDataCollector.Add(requestedArticle.Id.ToString());

        return new GetArticleCrossesResult(crossArticlesAdapted, requestedAdapted);
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