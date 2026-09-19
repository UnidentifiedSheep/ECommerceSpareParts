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

	public GetSupplierProductCrossesBatchQuery(IEnumerable<int> ids)
	{
		Ids = ids.ToHashSet();
	}
	public IReadOnlySet<int> Ids { get; }
}

public record GetSupplierProductCrossesBatchResult(Dictionary<int, List<SupplierProductDto>> Crosses);

public class GetSupplierProductCrossesBatchHandler(
	IReadRepository<SupplierProductCross, SupplierProductCrossKey> repository,
	IProjectionProvider<SupplierProduct, SupplierProductDto> projection)
	: IQueryHandler<GetSupplierProductCrossesBatchQuery, GetSupplierProductCrossesBatchResult>
{
	public async Task<GetSupplierProductCrossesBatchResult> Handle(
		GetSupplierProductCrossesBatchQuery request,
		CancellationToken cancellationToken)
	{
		var crosses = await repository
			.Query
			.AsExpandable()
			.Where(x => request.Ids.Contains(x.LeftId) || request.Ids.Contains(x.RightId))
			.Select(x => new
			{
				x.LeftId,
				x.RightId,
				Left = projection.Projection.Invoke(x.Left),
				Right = projection.Projection.Invoke(x.Right)
			})
			.ToListAsync(cancellationToken);

		var found = crosses
			.SelectMany(x =>
			{
				var result = new List<(int Id, SupplierProductDto Item)>(2);

				if (request.Ids.Contains(x.LeftId))
					result.Add((x.LeftId, x.Right));

				if (request.Ids.Contains(x.RightId))
					result.Add((x.RightId, x.Left));

				return result;
			})
			.ToLookup(x => x.Id, x => x.Item);

		var res = request.Ids.ToDictionary(id => id, id => found[id].ToList());

		return new GetSupplierProductCrossesBatchResult(res);
	}
}
