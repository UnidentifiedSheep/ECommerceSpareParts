using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetFullPurchase;

public record GetFullPurchaseQuery(Guid PurchaseId) : IQuery<GetFullPurchaseResult>;

public record GetFullPurchaseResult(PurchaseDto Purchase, IEnumerable<PurchaseContentDto> Contents);

public class GetFullPurchaseHandler(
	IReadRepository<Purchase, Guid> readRepository,
	IProjectionProvider<Purchase, PurchaseDto> purchaseProjection,
	IProjectionProvider<PurchaseContent, PurchaseContentDto> contentProjection)
	: IQueryHandler<GetFullPurchaseQuery, GetFullPurchaseResult>
{
	public async Task<GetFullPurchaseResult> Handle(
		GetFullPurchaseQuery request,
		CancellationToken cancellationToken)
	{
		var purchaseToDto = purchaseProjection.Projection;
		var contentToDto = contentProjection.Projection;

		var result = await readRepository
			.Query
			.Where(x => x.Id == request.PurchaseId)
			.AsExpandable()
			.Select(x => new
			{
				purchase = purchaseToDto.Invoke(x), contents = x.Contents.Select(z => contentToDto.Invoke(z))
			})
			.FirstOrDefaultAsync(cancellationToken) ??
		throw new PurchaseNotFoundException(request.PurchaseId);

		return new GetFullPurchaseResult(result.purchase, result.contents);
	}
}
