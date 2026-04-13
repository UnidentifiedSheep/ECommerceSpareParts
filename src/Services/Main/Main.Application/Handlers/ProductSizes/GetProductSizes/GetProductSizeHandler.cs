using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.ArticleSizes;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ArticleSizes.GetArticleSizes;

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