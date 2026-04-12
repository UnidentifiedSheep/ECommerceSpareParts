using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Application.Interfaces.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ArticlePairs.GetArticlePair;

public record GetProductPairQuery(int ProductId) : IQuery<GetProductPairResult>;

public record GetProductPairResult(ProductDto? Pair);

public class GetProductPairHandler(IReadDContext context)
    : IQueryHandler<GetProductPairQuery, GetProductPairResult>
{
    public async Task<GetProductPairResult> Handle(GetProductPairQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .Include(x => x.Pair)
            .ThenInclude(p => p!.Producer)
            .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

        if (product?.Pair == null) return new GetProductPairResult(null);
        
        var adapted = product.Pair.Adapt<ProductDto>();
        return new GetProductPairResult(adapted);
    }
}