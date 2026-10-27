using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases;

public sealed record GetPurchasesContentsByIdsQuery : IQuery<GetPurchasesContentsByIdsResult>
{
	public GetPurchasesContentsByIdsQuery(Guid id) : this([id]) { }

	public GetPurchasesContentsByIdsQuery(IEnumerable<Guid> ids)
	{
		Ids = ids.Distinct().ToArray();
	}

	public IReadOnlyList<Guid> Ids { get; }
}

public sealed record GetPurchasesContentsByIdsResult(IReadOnlyDictionary<Guid, IReadOnlyList<PurchaseContentDto>> Contents);

public sealed class GetPurchasesContentsByIdsHandler(
	IReadRepository<PurchaseContent, int> repository,
	IProjectionProvider<PurchaseContent, PurchaseContentDto> projection)
	: IQueryHandler<GetPurchasesContentsByIdsQuery, GetPurchasesContentsByIdsResult>
{
	public async Task<GetPurchasesContentsByIdsResult> Handle(
		GetPurchasesContentsByIdsQuery request,
		CancellationToken cancellationToken)
	{
		if (request.Ids.Count == 0)
			return new GetPurchasesContentsByIdsResult(new Dictionary<Guid, IReadOnlyList<PurchaseContentDto>>());

		var rows = await repository.Query
			.AsExpandable()
			.Where(content => request.Ids.Contains(content.PurchaseId))
			.OrderBy(content => content.Id)
			.Select(content => new
			{
				ParentId = content.PurchaseId,
				Dto = projection.Projection.Invoke(content)
			})
			.ToListAsync(cancellationToken);

		var contents = rows
			.GroupBy(row => row.ParentId, row => row.Dto)
			.ToDictionary(
				group => group.Key,
				IReadOnlyList<PurchaseContentDto> (group) => group.ToArray());

		return new GetPurchasesContentsByIdsResult(contents);
	}
}
