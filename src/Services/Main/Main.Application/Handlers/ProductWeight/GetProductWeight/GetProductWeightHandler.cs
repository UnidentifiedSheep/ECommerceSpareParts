using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductWeight.GetProductWeight;

public record GetProductWeightQuery(int ProductId) : IQuery<GetProductWeightResult>;

public record GetProductWeightResult(ProductWeightDto ProductWeight);

public class GetProductWeightHandler(IReadRepository<Entities.Product.ProductWeight, int> context)
    : IQueryHandler<GetProductWeightQuery, GetProductWeightResult>
{
    public async Task<GetProductWeightResult> Handle(
        GetProductWeightQuery request,
        CancellationToken cancellationToken)
    {
        var productWeight = await context.Query
                                .Where(x => x.ProductId == request.ProductId)
                                .Select(ProductProjections.ToProductWeightDto)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new ProductWeightNotFoundException(request.ProductId);

        return new GetProductWeightResult(productWeight);
    }
}