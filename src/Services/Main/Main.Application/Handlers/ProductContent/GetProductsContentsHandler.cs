using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductContent;

public record GetProductsContentsQuery : IQuery<GetProductsContentsResult>
{
    public IReadOnlyList<int> Ids { get; }

    public GetProductsContentsQuery(IEnumerable<int> ids)
    {
        Ids = ids.Distinct().ToList();
    }

    public GetProductsContentsQuery(int id) : this([id]) { }
}

public record GetProductsContentsResult(Dictionary<int, List<ProductContentDto>> Contents);

public class GetProductsContentsHandler(
    IReadRepository<Entities.Product.ProductContent, (int, int)> repository,
    IProjectionProvider<Product, ProductDto> productProjection
) : IQueryHandler<GetProductsContentsQuery, GetProductsContentsResult>
{
    public async Task<GetProductsContentsResult> Handle(
        GetProductsContentsQuery request,
        CancellationToken cancellationToken)
    {
        var productToDto = productProjection.Projection;

        var result = (await repository.Query
            .Where(x => request.Ids.Contains(x.ParentProductId))
            .AsExpandable()
            .Select(x => new
            {
                ParentId = x.ParentProductId,
                Dto = new ProductContentDto
                {
                    Quantity = x.Quantity,
                    Product = productToDto.Invoke(x.ChildProduct)
                }
            })
            .ToListAsync(cancellationToken))
            .GroupBy(x => x.ParentId, x => x.Dto)
            .ToDictionary(x => x.Key, x => x.ToList());
        
        return new GetProductsContentsResult(result);
    }
}
