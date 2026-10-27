using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public sealed record GetSalesByIdsQuery : IQuery<GetSalesByIdsResult>
{
	public GetSalesByIdsQuery(Guid id) : this([id]) { }

	public GetSalesByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetSalesByIdsResult(IReadOnlyDictionary<Guid, SaleDto> Sales);

public sealed class GetSalesByIdsHandler(
	IReadRepository<Sale, Guid> repository,
	IProjectionProvider<Sale, SaleDto> projection)
	: IQueryHandler<GetSalesByIdsQuery, GetSalesByIdsResult>
{
	public async Task<GetSalesByIdsResult> Handle(
		GetSalesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetSalesByIdsResult(new Dictionary<Guid, SaleDto>());

		var sales = await repository.Query
			.Where(sale => request.Ids.Contains(sale.Id))
			.Project(projection)
			.ToDictionaryAsync(sale => sale.Id, cancellationToken);

		return new GetSalesByIdsResult(sales);
	}
}
