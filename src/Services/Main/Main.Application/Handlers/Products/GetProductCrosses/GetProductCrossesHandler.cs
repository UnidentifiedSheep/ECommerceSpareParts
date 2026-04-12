using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Mapster;

namespace Main.Application.Handlers.Articles.GetArticleCrosses;

public record GetProductCrossesQuery(int ArticleId, PaginationModel Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IEnumerable<ProductDto> Crosses, ProductDto RequestedProduct);

public class GetProductCrossesHandler(
    IProductRepository repository,
    IRelatedDataCollector relatedDataCollector)
    : IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
    public async Task<GetProductCrossesResult> Handle(
        GetProductCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await GetRequestedArticle(request.ArticleId, cancellationToken);
        
        var crosses = await GetCrosses(request.ArticleId, pagination, request.SortBy, cancellationToken);

        var requestedAdapted = requestedArticle.Adapt<ProductDto>();
        var crossArticlesAdapted = crosses.Adapt<List<ProductDto>>();

        relatedDataCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        relatedDataCollector.Add(requestedArticle.Id.ToString());

        return new GetProductCrossesResult(crossArticlesAdapted, requestedAdapted);
    }

    private async Task<Product> GetRequestedArticle(int id, CancellationToken token)
    {
        var criteria = Criteria<Product>.New()
            .Track(false)
            .Include(x => x.Producer)
            .Where(x => x.Id == id)
            .Build();
        
        var requestedArticle = await repository.FirstOrDefaultAsync(criteria, token)
                               ?? throw new ProductNotFoundException(id);
        
        return requestedArticle;
    }

    private async Task<IReadOnlyList<Product>> GetCrosses(
        int articleId, 
        PaginationModel pagination,
        string? sortBy,
        CancellationToken token)
    {
        var criteria = Criteria<Product>.New()
            .Track(false)
            .Include(x => x.Producer)
            .Page(pagination.Page)
            .Size(pagination.Size)
            .WithSorting(sortBy)
            .Build();
        
        return await repository.GetProductCrosses(articleId, criteria, token);
    }
}