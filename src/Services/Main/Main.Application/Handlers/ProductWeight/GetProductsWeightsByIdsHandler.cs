using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductWeight;

public record GetProductsWeightsByIdsQuery : IQuery<GetProductsWeightsByIdsResult>
{
    public IReadOnlyList<int> Ids { get; }

    public GetProductsWeightsByIdsQuery(IEnumerable<int> ids)
    {
        Ids = ids.Distinct().ToList();
    }

    public GetProductsWeightsByIdsQuery(int id)
    {
        Ids = [id];
    }
}

public record GetProductsWeightsByIdsResult(IReadOnlyList<ProductWeightDto> Weights);

public class GetProductsWeightsByIdsHandler(
    IReadRepository<Entities.Product.ProductWeight, int> repository,
    IProjectionProvider<Entities.Product.ProductWeight, ProductWeightDto> projectionProvider
    ) : IQueryHandler<GetProductsWeightsByIdsQuery, GetProductsWeightsByIdsResult>
{
    public async Task<GetProductsWeightsByIdsResult> Handle(GetProductsWeightsByIdsQuery request, CancellationToken cancellationToken)
    {
        var results = await repository.Query
            .Where(x => request.Ids.Contains(x.ProductId))
            .Project(projectionProvider)
            .ToListAsync(cancellationToken);
        
        return new GetProductsWeightsByIdsResult(results);
    }
}