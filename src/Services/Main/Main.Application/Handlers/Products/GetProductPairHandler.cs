using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Application.Extensions.QueryExtensions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetProductPairQuery(int ProductId) : IQuery<GetProductPairResult>;

public record GetProductPairResult(ProductDto? Pair);

public class GetProductPairHandler(IReadRepository<Product, int> context)
    : IQueryHandler<GetProductPairQuery, GetProductPairResult>
{
    public async Task<GetProductPairResult> Handle(
        GetProductPairQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Query
            .Where(x => x.Id == request.ProductId && x.PairId != null)
            .Include(x => x.Pair)
            .FirstProductDtoAsync(cancellationToken: cancellationToken);

        return new GetProductPairResult(product);
    }
}