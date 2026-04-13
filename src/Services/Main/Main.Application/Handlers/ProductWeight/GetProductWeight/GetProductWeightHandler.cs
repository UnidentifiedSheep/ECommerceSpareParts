using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.ArticleWeight;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ArticleWeight.GetArticleWeight;

public record GetProductWeightQuery(int ProductId) : IQuery<GetProductWeightResult>;

public record GetProductWeightResult(ProductWeightDto ProductWeight);

public class GetProductWeightHandler(IReadRepository<ProductWeight, int> context)
    : IQueryHandler<GetProductWeightQuery, GetProductWeightResult>
{
    public async Task<GetProductWeightResult> Handle(GetProductWeightQuery request, CancellationToken cancellationToken)
    {
        var productWeight = await context.Query
            .Select(x => new ProductWeightDto
            {
                ProductId = x.ProductId,
                Weight = x.Weight,
                Unit = x.Unit,
            })
            .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken)
                ?? throw new ProductWeightNotFoundException(request.ProductId);

        return new GetProductWeightResult(productWeight);
    }
}