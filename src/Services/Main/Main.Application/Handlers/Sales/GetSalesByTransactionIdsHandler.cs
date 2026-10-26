using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Sale;
using Main.Entities.Sale;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Sales;

public sealed record GetSalesByTransactionIdsQuery : IQuery<GetSalesByTransactionIdsResult>
{
	public GetSalesByTransactionIdsQuery(Guid id) : this([id]) { }

	public GetSalesByTransactionIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetSalesByTransactionIdsResult(IReadOnlyDictionary<Guid, SaleDto> Sales);

public sealed class GetSalesByTransactionIdsHandler(
	IReadRepository<Sale, Guid> repository,
	IProjectionProvider<Sale, SaleDto> projection)
	: IQueryHandler<GetSalesByTransactionIdsQuery, GetSalesByTransactionIdsResult>
{
	public async Task<GetSalesByTransactionIdsResult> Handle(
		GetSalesByTransactionIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetSalesByTransactionIdsResult(new Dictionary<Guid, SaleDto>());

		var sales = await repository.Query
			.Where(sale => request.Ids.Contains(sale.TransactionId))
			.Project(projection)
			.ToDictionaryAsync(sale => sale.TransactionId, cancellationToken);

		return new GetSalesByTransactionIdsResult(sales);
	}
}
