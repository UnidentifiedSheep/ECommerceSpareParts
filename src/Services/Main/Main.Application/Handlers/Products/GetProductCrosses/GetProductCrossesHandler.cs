using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Extensions.QueryExtensions;
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Cache;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public record GetProductCrossesQuery(int ProductId, Pagination Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IReadOnlyList<ProductDto> Crosses, ProductDto RequestedProduct);

public class GetProductCrossesHandler(
    IProductCacheRepository productCache,
    IReadRepository<Product, int> productRepository,
    IReadRepository<ProductCross, (int, int)> crossesRepository,
    IIdsCollector idsCollector)
    : IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
    public async Task<GetProductCrossesResult> Handle(
        GetProductCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await productCache.GetProductOrSetAsync(request.ProductId, cancellationToken);

        var crosses = await GetCrosses(request.ProductId, pagination, request.SortBy, cancellationToken);

        idsCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        idsCollector.Add(requestedArticle.Id.ToString());

        return new GetProductCrossesResult(crosses, requestedArticle);
    }

    private async Task<ProductDto> GetRequestedArticle(int id, CancellationToken token)
    {
        return await productRepository.Query
            .Where(x => x.Id == id)
            .AsExpandable()
            .Select(ProductProjections.ToDto)
            .FirstOrDefaultAsync(token) ?? throw new ProductNotFoundException(id);
    }

    private async Task<IReadOnlyList<ProductDto>> GetCrosses(
        int productId,
        Pagination pagination,
        string? sortBy,
        CancellationToken token)
    {
        return await crossesRepository.Query
            .GetCrosses(productId)
            .SortBy(sortBy)
            .ApplyPagination(pagination)
            .AsExpandable()
            .Select(ProductProjections.ToDto)
            .ToListAsync(token);
    }
}