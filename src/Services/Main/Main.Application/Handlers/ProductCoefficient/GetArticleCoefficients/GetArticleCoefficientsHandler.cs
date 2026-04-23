using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Amw.ArticleCoefficients;
using Main.Application.Handlers.Projections;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductCoefficient.GetArticleCoefficients;

public record GetArticleCoefficientsQuery(IEnumerable<int> ProductIds) : IQuery<GetArticleCoefficientsResult>;

public record GetArticleCoefficientsResult(List<ProductCoefficientDto> Coefficients);

public class GetArticleCoefficientsHandler(
    IReadRepository<Entities.Product.ProductCoefficient, (int, string)> repository)
    : IQueryHandler<GetArticleCoefficientsQuery, GetArticleCoefficientsResult>
{
    public async Task<GetArticleCoefficientsResult> Handle(
        GetArticleCoefficientsQuery request,
        CancellationToken cancellationToken)
    {
        var coefficients = await repository.Query
            .Where(x => request.ProductIds.Contains(x.ProductId))
            .AsExpandable()
            .Select(ProductProjections.ToProductCoefficientDto)
            .ToListAsync(cancellationToken);

        return new GetArticleCoefficientsResult(coefficients);
    }
}