using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetFullProductsQuery : IQuery<GetFullProductsResult>
{
    public IReadOnlyList<int> ProductIds { get; }
    
    public GetFullProductsQuery(IEnumerable<int> ids)
    {
        ProductIds = ids.Distinct().ToList();
    }
}

public record GetFullProductsResult(IReadOnlyList<FullProductDto> Products);

public class GetFullProductsHandler(
    IReadRepository<Product, int> repository
) : IQueryHandler<GetFullProductsQuery, GetFullProductsResult>
{
    public async Task<GetFullProductsResult> Handle(
        GetFullProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0) return new GetFullProductsResult([]);
        
        var result = await repository
            .Query
            .Where(x => request.ProductIds.Contains(x.Id))
            .AsExpandable()
            .Select(ProductProjections.ToFullDto)
            .ToListAsync(cancellationToken);

        return new GetFullProductsResult(result);
    }
}