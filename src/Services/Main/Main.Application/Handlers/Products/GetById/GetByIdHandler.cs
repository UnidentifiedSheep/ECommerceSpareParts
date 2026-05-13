using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Product;
using Main.Application.Interfaces.Cache;

namespace Main.Application.Handlers.Products.GetById;

public record GetByIdQuery(int ProductId) : IQuery<GetByIdResult>;

public record GetByIdResult(ProductDto Product);

public class GetByIdHandler(IProductCacheRepository cacheRepository) : IQueryHandler<GetByIdQuery, GetByIdResult>
{
    public async Task<GetByIdResult> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await cacheRepository
            .GetProductOrSetAsync(request.ProductId, cancellationToken);
        return new GetByIdResult(product);
    }
}