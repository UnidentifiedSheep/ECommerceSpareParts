using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public sealed record GetSaleContentDetailsByIdsQuery : IQuery<GetSaleContentDetailsByIdsResult>
{
	public GetSaleContentDetailsByIdsQuery(int id) : this([id]) { }

	public GetSaleContentDetailsByIdsQuery(IEnumerable<int> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<int> Ids { get; }
}

public sealed record GetSaleContentDetailsByIdsResult(IReadOnlyDictionary<int, IReadOnlyList<SaleContentDetailDto>> Details);

public sealed class GetSaleContentDetailsByIdsHandler(
	IReadRepository<SaleContentDetail, int> repository,
	IProjectionProvider<SaleContentDetail, SaleContentDetailDto> projection)
	: IQueryHandler<GetSaleContentDetailsByIdsQuery, GetSaleContentDetailsByIdsResult>
{
	public async Task<GetSaleContentDetailsByIdsResult> Handle(
		GetSaleContentDetailsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetSaleContentDetailsByIdsResult(new Dictionary<int, IReadOnlyList<SaleContentDetailDto>>());

		var rows = await repository.Query
			.AsExpandable()
			.Where(content => request.Ids.Contains(content.SaleContentId))
			.OrderBy(content => content.Id)
			.Select(content => new
			{
				ParentId = content.SaleContentId,
				Dto = projection.Projection.Invoke(content)
			})
			.ToListAsync(cancellationToken);

		var details = rows
			.GroupBy(row => row.ParentId, row => row.Dto)
			.ToDictionary(
				group => group.Key,
				IReadOnlyList<SaleContentDetailDto> (group) => group.ToArray());

		return new GetSaleContentDetailsByIdsResult(details);
	}
}
