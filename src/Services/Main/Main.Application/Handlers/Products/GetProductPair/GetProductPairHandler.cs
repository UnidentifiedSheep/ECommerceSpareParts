using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products.GetProductPair;

public record GetProductPairQuery(int ProductId) : IQuery<GetProductPairResult>;

public record GetProductPairResult(ProductDto? Pair);

public class GetProductPairHandler(IReadRepository<Product, int> context)
    : IQueryHandler<GetProductPairQuery, GetProductPairResult>
{
    public async Task<GetProductPairResult> Handle(GetProductPairQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Query
            .Include(x => x.Pair)
            .ThenInclude(p => p!.Producer)
            .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product?.Pair == null) return new GetProductPairResult(null);
        
        var adapted = product.Pair.Adapt<ProductDto>();
        return new GetProductPairResult(adapted);
    }
}