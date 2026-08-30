using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Product;
using Main.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductSizes;

public record GetProductsSizesByIdsQuery : IQuery<GetProductsSizesByIdsResult>
{

	public GetProductsSizesByIdsQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToList();
	}

	public GetProductsSizesByIdsQuery(int id)
	{
		Ids = new List<int>
		{
			id
		};
	}

	public IReadOnlyList<int> Ids { get; }
}

public record GetProductsSizesByIdsResult(IReadOnlyList<ProductSizeDto> Sizes);

public class GetProductsSizesByIdsHandler(
	IReadRepository<ProductSize, int> repository,
	IProjectionProvider<ProductSize, ProductSizeDto> projectionProvider)
	: IQueryHandler<GetProductsSizesByIdsQuery, GetProductsSizesByIdsResult>
{
	public async Task<GetProductsSizesByIdsResult> Handle(
		GetProductsSizesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		var result = await repository
			.Query
			.Where(x => request.Ids.Contains(x.ProductId))
			.Project(projectionProvider)
			.ToListAsync(cancellationToken);

		return new GetProductsSizesByIdsResult(result);
	}
}
