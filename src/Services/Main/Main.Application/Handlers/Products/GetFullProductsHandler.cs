using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Products;

public record GetFullProductsQuery : IQuery<GetFullProductsResult>
{

	public GetFullProductsQuery(IEnumerable<int> ids)
	{
		ProductIds = ids.Distinct().ToList();
	}

	public IReadOnlyList<int> ProductIds { get; }
}

public record GetFullProductsResult(IReadOnlyList<FullProductDto> Products);

public class GetFullProductsHandler(
	IReadRepository<Product, int> repository,
	IProjectionProvider<Product, FullProductDto> productProjection)
	: IQueryHandler<GetFullProductsQuery, GetFullProductsResult>
{
	public async Task<GetFullProductsResult> Handle(
		GetFullProductsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.ProductIds.Count == 0)
			return new GetFullProductsResult([]);

		var result = await repository
			.Query
			.Where(x => request.ProductIds.Contains(x.Id))
			.Project(productProjection)
			.ToListAsync(cancellationToken);

		return new GetFullProductsResult(result);
	}
}
