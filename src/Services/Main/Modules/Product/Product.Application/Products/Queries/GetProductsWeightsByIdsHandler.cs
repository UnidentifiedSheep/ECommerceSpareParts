using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Products.Queries;

public record GetProductsWeightsByIdsQuery : IQuery<GetProductsWeightsByIdsResult>
{

	public GetProductsWeightsByIdsQuery(IEnumerable<int> ids) => Ids = ids.Distinct().ToList();

	public GetProductsWeightsByIdsQuery(int id) => Ids = [id];

	public IReadOnlyList<int> Ids { get; }
}

public record GetProductsWeightsByIdsResult(IReadOnlyList<ProductWeightDto> Weights);

public class GetProductsWeightsByIdsHandler(
	IReadRepository<ProductWeight, int> repository,
	IProjectionProvider<ProductWeight, ProductWeightDto> projectionProvider)
	: IQueryHandler<GetProductsWeightsByIdsQuery, GetProductsWeightsByIdsResult>
{
	public async Task<GetProductsWeightsByIdsResult> Handle(
		GetProductsWeightsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var results = await repository
			.Query
			.Where(x => request.Ids.Contains(x.ProductId))
			.Project(projectionProvider)
			.ToListAsync(cancellationToken);

		return new GetProductsWeightsByIdsResult(results);
	}
}
