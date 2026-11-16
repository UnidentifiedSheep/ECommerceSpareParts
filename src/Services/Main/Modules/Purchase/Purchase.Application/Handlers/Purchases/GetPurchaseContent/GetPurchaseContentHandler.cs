using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Entities.Purchase.Exceptions;
using Main.Entities.Purchase.Purchase;
using Main.Modules.Purchase.Application.Dtos.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Modules.Purchase.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(Guid Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(IReadOnlyList<PurchaseContentDto> Content);

public class GetPurchaseContentHandler(
	IReadRepository<Entities.Purchase.Purchase.Purchase, Guid> repository,
	IProjectionProvider<PurchaseContent, PurchaseContentDto> contentProjection)
	: IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
	public async Task<GetPurchaseContentResult> Handle(
		GetPurchaseContentQuery request,
		CancellationToken cancellationToken)
	{
		var contentToDto = contentProjection.Projection;

		var result = await repository
			.Query
			.Where(x => x.Id == request.Id)
			.AsExpandable()
			.Select(x => x.Contents.Select(z => contentToDto.Invoke(z)))
			.FirstOrDefaultAsync(cancellationToken) ?? throw new PurchaseNotFoundException(request.Id);
		return new GetPurchaseContentResult(result.ToList());
	}
}
