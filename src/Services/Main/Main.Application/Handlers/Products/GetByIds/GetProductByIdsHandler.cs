using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Cache;

namespace Main.Application.Handlers.Products.GetByIds;

public record GetProductByIdsQuery(IEnumerable<int> Ids) : IQuery<GetProductByIdsResult>;
public record GetProductByIdsResult(IReadOnlyList<ProductDto> Products);

public class GetProductByIdsHandler(
    IProductCacheRepository cacheRepository
) : IQueryHandler<GetProductByIdsQuery, GetProductByIdsResult>
{
    public async Task<GetProductByIdsResult> Handle(
        GetProductByIdsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await cacheRepository
            .GetProductsOrSetAsync(request.Ids, cancellationToken);
        return new GetProductByIdsResult(result.Values.ToList());
    }
}