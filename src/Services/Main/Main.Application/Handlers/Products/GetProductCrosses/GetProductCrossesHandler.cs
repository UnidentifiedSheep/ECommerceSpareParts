using Abstractions.Models;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Products;

namespace Main.Application.Handlers.Products.GetProductCrosses;

public record GetProductCrossesQuery(
    int ProductId,
    Pagination Pagination,
    string[] SortBy,
    Guid? UserId
) : IQuery<GetProductCrossesResult>;

public record GetProductCrossesResult(IReadOnlyList<ProductDto> Crosses);

public class GetProductCrossesHandler(
    IProductProvider productProvider
)
    : IQueryHandler<GetProductCrossesQuery, GetProductCrossesResult>
{
    public async Task<GetProductCrossesResult> Handle(
        GetProductCrossesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = request.Pagination;

        var crosses = await GetCrosses(
            request.ProductId,
            pagination,
            request.SortBy,
            cancellationToken);

        return new GetProductCrossesResult(crosses);
    }

    private async Task<IReadOnlyList<ProductDto>> GetCrosses(
        int productId,
        Pagination pagination,
        string[] sortBy,
        CancellationToken token)
    {
        var crosseIds = (await productProvider.GetProductCrossesAsync(
                productId,
                sortBy,
                token))
            .ApplyPagination(pagination);

        return (await productProvider.GetProductsOrSetAsync(crosseIds, token)).Values.ToList();
    }
}
