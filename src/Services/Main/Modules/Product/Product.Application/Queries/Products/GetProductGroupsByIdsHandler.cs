using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Entities.Product.Product;
using Main.Modules.Product.Application.Dtos.Product;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Product.Application.Queries.Products;

public sealed record GetProductGroupsByIdsQuery : IQuery<GetProductGroupsByIdsResult>
{

	public GetProductGroupsByIdsQuery(int id) : this([id])
	{
	}

	public GetProductGroupsByIdsQuery(IEnumerable<int> ids) => Ids = ids.Distinct().ToArray();
	public IReadOnlyList<int> Ids { get; }
}

public sealed record GetProductGroupsByIdsResult(IReadOnlyDictionary<int, ProductGroupDto> Groups);

public sealed class GetProductGroupsByIdsHandler(
	IReadRepository<ProductGroup, int> repository,
	IProjectionProvider<ProductGroup, ProductGroupDto> projection)
	: IQueryHandler<GetProductGroupsByIdsQuery, GetProductGroupsByIdsResult>
{
	public async Task<GetProductGroupsByIdsResult> Handle(
		GetProductGroupsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetProductGroupsByIdsResult(new Dictionary<int, ProductGroupDto>());

		var groups = await repository.Query
			.Where(group => request.Ids.Contains(group.Id))
			.Project(projection)
			.ToDictionaryAsync(group => group.Id, cancellationToken);

		return new GetProductGroupsByIdsResult(groups);
	}
}
