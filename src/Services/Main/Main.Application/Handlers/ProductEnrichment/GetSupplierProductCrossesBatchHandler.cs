using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Product.Enrichment;
using Main.Entities.Product.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.ProductEnrichment;

public record GetSupplierProductCrossesBatchQuery : IQuery<GetSupplierProductCrossesBatchResult>
{
	public IReadOnlyList<int> Ids { get; }

	public GetSupplierProductCrossesBatchQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToList();
	}
}

public record GetSupplierProductCrossesBatchResult(
	Dictionary<int, List<SupplierProductDto>> Crosses);

public class GetSupplierProductCrossesBatchHandler(
	IReadRepository<SupplierProductCross, SupplierProductCrossKey> repository,
	IProjectionProvider<SupplierProduct, SupplierProductDto> projection
	) : IQueryHandler<GetSupplierProductCrossesBatchQuery, GetSupplierProductCrossesBatchResult>
{
	public async Task<GetSupplierProductCrossesBatchResult> Handle(
		GetSupplierProductCrossesBatchQuery request,
		CancellationToken cancellationToken)
	{
		var query = repository.Query.AsExpandable();

		var found = (await query
			.Where(x => request.Ids.Contains(x.LeftId))
			.Select(x => new
			{
				Id = x.LeftId, Item = projection.Projection.Invoke(x.Right)
			})
			.Concat(
				query
					.Where(x => request.Ids.Contains(x.RightId))
					.Select(x => new
					{
						Id = x.RightId, Item = projection.Projection.Invoke(x.Left)
					}))
			.ToListAsync(cancellationToken))
			.ToLookup(x => x.Id, x => x.Item);

		var res = request.Ids.ToDictionary(
			id => id,
			id => found[id].ToList());

		return new GetSupplierProductCrossesBatchResult(res);
	}
}
