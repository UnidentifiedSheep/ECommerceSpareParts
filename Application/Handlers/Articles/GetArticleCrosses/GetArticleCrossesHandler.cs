using Application.Interfaces;
using Core.Interfaces.DbRepositories;
using Core.Models;
using Exceptions.Exceptions.Articles;
using Mapster;


namespace Application.Handlers.Articles.GetArticleCrosses;

public record GetArticleCrossesQuery<TDto>(int ArticleId, PaginationModel Pagination, string? SortBy, string? UserId) : IQuery<GetArticleCrossesResult<TDto>>;

public record GetArticleCrossesResult<TDto>(IEnumerable<TDto> Crosses, TDto RequestedArticle);


public class GetArticleCrossesHandler<TDto>(IArticlesRepository articlesRepository) : IQueryHandler<GetArticleCrossesQuery<TDto>, GetArticleCrossesResult<TDto>>
{
    public async Task<GetArticleCrossesResult<TDto>> Handle(GetArticleCrossesQuery<TDto> request, CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await articlesRepository.GetArticleById(request.ArticleId, false, cancellationToken);
        if (requestedArticle == null) throw new ArticleNotFoundException(request.ArticleId);
        var crosses = await articlesRepository.GetArticleCrosses(request.ArticleId, pagination.Page, 
            pagination.Size, request.SortBy, false, cancellationToken);
        
        TDto requestedAdapted = requestedArticle.Adapt<TDto>();
        List<TDto> crossArticlesAdapted = crosses.Adapt<List<TDto>>();
        
        return new GetArticleCrossesResult<TDto>(crossArticlesAdapted, requestedAdapted);
    }
}