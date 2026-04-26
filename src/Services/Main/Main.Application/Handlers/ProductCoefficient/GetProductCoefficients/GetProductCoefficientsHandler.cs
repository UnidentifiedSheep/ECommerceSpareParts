using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Amw.ArticleCoefficients;
using Main.Application.Handlers.Projections;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductCoefficient.GetArticleCoefficients;

public record GetProductCoefficientsQuery(IEnumerable<int> ProductIds) : IQuery<GetProductCoefficientsResult>;

public record GetProductCoefficientsResult(List<ProductCoefficientDto> Coefficients);

public class GetProductCoefficientsHandler(
    IReadRepository<Entities.Product.ProductCoefficient, (int, string)> repository)
    : IQueryHandler<GetProductCoefficientsQuery, GetProductCoefficientsResult>
{
    public async Task<GetProductCoefficientsResult> Handle(
        GetProductCoefficientsQuery request,
        CancellationToken cancellationToken)
    {
        var coefficients = await repository.Query
            .Where(x => request.ProductIds.Contains(x.ProductId))
            .AsExpandable()
            .Select(ProductProjections.ToProductCoefficientDto)
            .ToListAsync(cancellationToken);

        return new GetProductCoefficientsResult(coefficients);
    }
}