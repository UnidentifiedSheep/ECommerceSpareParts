using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Exceptions.Products;
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
                       .Select(x => new ProductSizeDto 
                       {
                           ProductId = x.ProductId,
                           Length = x.Length,
                           Width = x.Width,
                           Height = x.Height,
                           Unit = x.Unit,
                           VolumeM3 = x.VolumeM3,
                       })
                       .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken)
                   ?? throw new ProductSizesNotFoundException(request.ProductId);
        
        return new GetProductSizesResult(size);
    }
}