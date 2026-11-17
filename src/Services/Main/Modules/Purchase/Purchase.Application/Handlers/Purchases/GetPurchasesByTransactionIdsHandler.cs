using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Modules.Purchase.Application.Dtos.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Purchase.Application.Handlers.Purchases;

public sealed record GetPurchasesByTransactionIdsQuery : IQuery<GetPurchasesByTransactionIdsResult>
{
	public GetPurchasesByTransactionIdsQuery(Guid id) : this([id]) { }

	public GetPurchasesByTransactionIdsQuery(IEnumerable<Guid> ids) => Ids = ids.Distinct().ToArray();

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetPurchasesByTransactionIdsResult(IReadOnlyDictionary<Guid, PurchaseDto> Purchases);

public sealed class GetPurchasesByTransactionIdsHandler(
	IReadRepository<Entities.Purchase.Purchase.Purchase, Guid> repository,
	IProjectionProvider<Entities.Purchase.Purchase.Purchase, PurchaseDto> projection)
	: IQueryHandler<GetPurchasesByTransactionIdsQuery, GetPurchasesByTransactionIdsResult>
{
	public async Task<GetPurchasesByTransactionIdsResult> Handle(
		GetPurchasesByTransactionIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetPurchasesByTransactionIdsResult(new Dictionary<Guid, PurchaseDto>());

		var purchases = await repository.Query
			.Where(purchase => request.Ids.Contains(purchase.TransactionId))
			.Project(projection)
			.ToDictionaryAsync(purchase => purchase.TransactionId, cancellationToken);

		return new GetPurchasesByTransactionIdsResult(purchases);
	}
}
