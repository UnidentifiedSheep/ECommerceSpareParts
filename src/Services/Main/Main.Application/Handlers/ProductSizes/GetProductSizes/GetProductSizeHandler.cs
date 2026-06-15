using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductSizes.GetProductSizes;

public record GetProductSizeQuery(int ProductId) : IQuery<GetProductSizesResult>;

public record GetProductSizesResult(ProductSizeDto ProductSize);

public class GetProductSizeHandler(IReadRepository<ProductSize, int> context)
    : IQueryHandler<GetProductSizeQuery, GetProductSizesResult>
{
    public async Task<GetProductSizesResult> Handle(GetProductSizeQuery request, CancellationToken cancellationToken)
    {
        var size = await context.Query
                       .Where(x => x.ProductId == request.ProductId)
                       .Select(ProductProjections.ToProductSizeDto)
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new ProductSizesNotFoundException(request.ProductId);

        return new GetProductSizesResult(size);
    }
}