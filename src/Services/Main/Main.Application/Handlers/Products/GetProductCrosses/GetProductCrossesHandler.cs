using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;
using Mapster;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public record GetProductCrossesQuery(int ProductId, Pagination Pagination, string? SortBy, Guid? UserId)
    : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IReadOnlyList<ProductDto> Crosses, ProductDto RequestedProduct);

public class GetProductCrossesHandler(
    IProductRepository repository,
    IIdsCollector idsCollector)
    : IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
    public async Task<GetProductCrossesResult> Handle(
        GetProductCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;
        var requestedArticle = await GetRequestedArticle(request.ProductId, cancellationToken);

        var crosses = await GetCrosses(request.ProductId, pagination, request.SortBy, cancellationToken);

        var requestedAdapted = requestedArticle.Adapt<ProductDto>();
        var crossArticlesAdapted = crosses.Adapt<List<ProductDto>>();

        idsCollector.AddRange(crosses.Select(x => x.Id.ToString()));
        idsCollector.Add(requestedArticle.Id.ToString());

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
        Pagination pagination,
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