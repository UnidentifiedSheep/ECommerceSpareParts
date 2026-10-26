using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public sealed record GetSalesContentsByIdsQuery : IQuery<GetSalesContentsByIdsResult>
{
	public GetSalesContentsByIdsQuery(Guid id) : this([id]) { }

	public GetSalesContentsByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetSalesContentsByIdsResult(IReadOnlyDictionary<Guid, IReadOnlyList<SaleContentDto>> Contents);

public sealed class GetSalesContentsByIdsHandler(
	IReadRepository<SaleContent, int> repository,
	IProjectionProvider<SaleContent, SaleContentDto> projection)
	: IQueryHandler<GetSalesContentsByIdsQuery, GetSalesContentsByIdsResult>
{
	public async Task<GetSalesContentsByIdsResult> Handle(
		GetSalesContentsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetSalesContentsByIdsResult(new Dictionary<Guid, IReadOnlyList<SaleContentDto>>());

		var rows = await repository.Query
			.AsExpandable()
			.Where(content => request.Ids.Contains(content.SaleId))
			.OrderBy(content => content.Id)
			.Select(content => new
			{
				ParentId = content.SaleId,
				Dto = projection.Projection.Invoke(content)
			})
			.ToListAsync(cancellationToken);

		var contents = rows.GroupBy(row => row.ParentId, row => row.Dto)
			.ToDictionary(group => group.Key, IReadOnlyList<SaleContentDto> (group) => group.ToArray());

		return new GetSalesContentsByIdsResult(contents);
	}
}
