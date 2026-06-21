using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetFullProductQuery(int ProductId) : IQuery<GetFullProductResult>;

public record GetFullProductResult(
    ProductDto Product,
    ProductWeightDto? ProductWeight,
    ProductSizeDto? ProductSize);

public class GetFullProductHandler(
    IReadRepository<Product, int> repository
) : IQueryHandler<GetFullProductQuery, GetFullProductResult>
{
    public async Task<GetFullProductResult> Handle(GetFullProductQuery request, CancellationToken cancellationToken)
    {
        var result = await repository
            .Query
            .Where(x => x.Id == request.ProductId)
            .AsExpandable()
            .Select(x => new
            {
                product = ProductProjections.ToDto.Invoke(x),
                weight = ProductProjections.ToProductWeightDto.Invoke(x.ProductWeight),
                size = ProductProjections.ToProductSizeDto.Invoke(x.ProductSize)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result?.product == null
            ? throw new ProductNotFoundException(request.ProductId)
            : new GetFullProductResult(result.product, result.weight, result.size);
    }
}
