using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Main.Application.Dtos.Purchase;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases;

public sealed record GetPurchasesByIdsQuery : IQuery<GetPurchasesByIdsResult>
{
	public GetPurchasesByIdsQuery(Guid id) : this([id]) { }

	public GetPurchasesByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetPurchasesByIdsResult(IReadOnlyDictionary<Guid, PurchaseDto> Purchases);

public sealed class GetPurchasesByIdsHandler(
	IReadRepository<Purchase, Guid> repository,
	IProjectionProvider<Purchase, PurchaseDto> projection)
	: IQueryHandler<GetPurchasesByIdsQuery, GetPurchasesByIdsResult>
{
	public async Task<GetPurchasesByIdsResult> Handle(
		GetPurchasesByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetPurchasesByIdsResult(new Dictionary<Guid, PurchaseDto>());

		var purchases = await repository.Query
			.Where(purchase => request.Ids.Contains(purchase.Id))
			.Project(projection)
			.ToDictionaryAsync(purchase => purchase.Id, cancellationToken);

		return new GetPurchasesByIdsResult(purchases);
	}
}
