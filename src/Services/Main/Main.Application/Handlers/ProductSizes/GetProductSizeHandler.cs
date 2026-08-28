using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductSizes;

public record GetProductSizeQuery(int ProductId) : IQuery<GetProductSizesResult>;

public record GetProductSizesResult(ProductSizeDto ProductSize);

public class GetProductSizeHandler(
    IReadRepository<ProductSize, int> context,
    IProjectionProvider<ProductSize, ProductSizeDto> projection)
    : IQueryHandler<GetProductSizeQuery, GetProductSizesResult>
{
    public async Task<GetProductSizesResult> Handle(
        GetProductSizeQuery request,
        CancellationToken cancellationToken)
    {
        var size = await context.Query
                       .Where(x => x.ProductId == request.ProductId)
                       .Project(projection)
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new ProductSizesNotFoundException(request.ProductId);

        return new GetProductSizesResult(size);
    }
}
