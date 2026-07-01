using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product;
using Main.Application.Projections;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductCoefficient.GetProductCoefficients;

public record GetProductCoefficientsQuery(IEnumerable<int> ProductIds) : IQuery<GetProductCoefficientsResult>;

public record GetProductCoefficientsResult(List<ProductCoefficientDto> Coefficients);

public class GetProductCoefficientsHandler(
    IReadRepository<Entities.Product.ProductCoefficient, (int, string)> repository
)
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