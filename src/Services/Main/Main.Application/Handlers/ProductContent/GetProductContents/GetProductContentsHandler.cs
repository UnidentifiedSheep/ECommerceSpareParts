using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Projections;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductContent.GetProductContents;

public record GetProductContentsQuery(int ProductId) : IQuery<GetProductContentsResult>;

public record GetProductContentsResult(IReadOnlyList<ProductContentDto> Contents);

public class GetProductContentsHandler(
    IReadRepository<Entities.Product.ProductContent, (int, int)> repository,
    IProjectionProvider<Product, ProductDto> productProjection
)
    : IQueryHandler<GetProductContentsQuery, GetProductContentsResult>
{
    public async Task<GetProductContentsResult> Handle(
        GetProductContentsQuery request,
        CancellationToken cancellationToken)
    {
        var productToDto = productProjection.Projection;

        var result = await repository.Query
            .Where(x => x.ParentProductId == request.ProductId)
            .AsExpandable()
            .Select(x => new ProductContentDto
            {
                Quantity = x.Quantity,
                Product = productToDto.Invoke(x.ChildProduct)
            })
            .ToListAsync(cancellationToken);
        return new GetProductContentsResult(result);
    }
}
