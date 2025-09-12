using Application.Interfaces;
using Core.Enums;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Mapster;

namespace Application.Handlers.Articles.GetArticles;

public record GetArticlesQuery<TDto>(
    string SearchTerm,
    PaginationModel Pagination,
    string? SortBy,
    IEnumerable<int> ProducerIds,
    ArticleSearchStrategy Strategy,
    string? UserId) : IQuery<GetArticlesResult<TDto>>;

public record GetArticlesResult<TDto>(IEnumerable<TDto> Articles);

public class GetArticlesHandler<TDto>(IArticlesRepository articlesRepository)
    : IQueryHandler<GetArticlesQuery<TDto>, GetArticlesResult<TDto>>
{
    public async Task<GetArticlesResult<TDto>> Handle(GetArticlesQuery<TDto> request,
        CancellationToken cancellationToken)
    {
        var page = request.Pagination.Page;
        var viewCount = request.Pagination.Size;

        var articles = request.Strategy switch
        {
            ArticleSearchStrategy.ByStartNumber =>
                await articlesRepository.GetArticlesByStartNumber(
                    request.SearchTerm, page, viewCount,
                    request.SortBy, request.ProducerIds, cancellationToken),

            ArticleSearchStrategy.ByExecNumber =>
                await articlesRepository.GetArticlesByExecNumber(
                    request.SearchTerm, page, viewCount,
                    request.SortBy, request.ProducerIds, cancellationToken),

            ArticleSearchStrategy.ByName =>
                await articlesRepository.GetArticlesByName(
                    request.SearchTerm, page, viewCount,
                    request.SortBy, request.ProducerIds, cancellationToken),

            ArticleSearchStrategy.ByArticleOrName =>
                await articlesRepository.GetArticlesByNameOrNumber(
                    request.SearchTerm, page, viewCount,
                    request.SortBy, request.ProducerIds, cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(request.Strategy), request.Strategy, null)
        };

        var dto = articles.Adapt<List<TDto>>();
        return new GetArticlesResult<TDto>(dto);
    }
}