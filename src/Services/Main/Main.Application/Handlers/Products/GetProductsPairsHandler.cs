using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
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

public record GetProductsPairsResult(IReadOnlyDictionary<int, ProductDto> Pairs);

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
            .AsExpandable()
            .Where(x => request.Ids.Contains(x.Id))
            .Where(x => x.PairId != null)
            .Select(x => new
            {
                Id = x.Id,
                Pair = productProjection.Projection.Invoke(x.Pair!)
            })
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Pair, 
                cancellationToken);

        return new GetProductsPairsResult(product);
    }
}
