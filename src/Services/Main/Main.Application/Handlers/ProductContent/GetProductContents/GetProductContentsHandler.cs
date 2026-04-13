using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Dtos.Anonymous.Articles;
using Main.Entities.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ArticleContent.GetArticleContents;

public record GetProductContentsQuery(int ProductId) : IQuery<GetProductContentsResult>;

public record GetProductContentsResult(IReadOnlyList<ProductContentDto> Contents);

public class GetProductContentsHandler(IReadRepository<ProductContent, (int, int)> repository)
    : IQueryHandler<GetProductContentsQuery, GetProductContentsResult>
{
    public async Task<GetProductContentsResult> Handle(
        GetProductContentsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository.Query
            .Where(x => x.ParentProductId == request.ProductId)
            .Select(x => new ProductContentDto
            {
                Quantity = x.Quantity,
                Product = new ProductDto
                {
                    Id = x.ChildProductId,
                    Description = x.ChildProduct.Description,
                    Indicator = x.ChildProduct.Indicator,
                    Name = x.ChildProduct.Name,
                    Stock = x.ChildProduct.Stock,
                    Sku = x.ChildProduct.Sku,
                    ProducerId = x.ChildProduct.ProducerId,
                    ProducerName = x.ChildProduct.Producer.Name,
                    Images = x.ChildProduct.Images.Select(z => z.Path).ToList()
                }
            }).ToListAsync(cancellationToken);
        return new GetProductContentsResult(result);
    }
}