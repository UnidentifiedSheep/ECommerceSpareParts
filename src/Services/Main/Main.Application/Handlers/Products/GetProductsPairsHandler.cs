using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetProductsPairsQuery : IQuery<GetProductsPairsResult>
{
    public IReadOnlyList<int> Ids { get; }

    public GetProductsPairsQuery(IEnumerable<int> ids)
    {
        Ids = ids.Distinct().ToList();
    }

    public GetProductsPairsQuery(int id) : this([id]) { }
}

public record GetProductsPairsResult(IReadOnlyList<ProductDto> Pairs);

public class GetProductsPairsHandler(
    IReadRepository<Product, int> context,
    IProjectionProvider<Product, ProductDto> productProjection)
    : IQueryHandler<GetProductsPairsQuery, GetProductsPairsResult>
{
    public async Task<GetProductsPairsResult> Handle(
        GetProductsPairsQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Query
            .Where(x => request.Ids.Contains(x.Id))
            .Where(x => x.PairId != null)
            .Select(x => x.Pair!)
            .Project(productProjection)
            .ToListAsync(cancellationToken);

        return new GetProductsPairsResult(product);
    }
}
