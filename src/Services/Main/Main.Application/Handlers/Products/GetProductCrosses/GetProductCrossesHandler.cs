using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Cache;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public record GetProductCrossesQuery(int ProductId, Pagination Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IReadOnlyList<ProductDto> Crosses, ProductDto RequestedProduct);

public class GetProductCrossesHandler(
    IProductCacheRepository productCache)
    : IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
    public async Task<GetProductCrossesResult> Handle(
        GetProductCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await productCache.GetProductOrSetAsync(
            request.ProductId, 
            cancellationToken);

        var crosses = await GetCrosses(
            request.ProductId, 
            pagination, 
            request.SortBy, 
            cancellationToken);

        return new GetProductCrossesResult(crosses, requestedArticle);
    }

    private async Task<IReadOnlyList<ProductDto>> GetCrosses(
        int productId,
        Pagination pagination,
        string? sortBy,
        CancellationToken token)
    {
        var crosseIds = (await productCache.GetProductCrossesAsync(
            productId,
            sortBy,
            token))
            .ApplyPagination(pagination);
        
        return (await productCache.GetProductsOrSetAsync(crosseIds, token)).Values.ToList();
    }
}