using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetPurchaseContent;

public record GetPurchaseContentQuery(Guid Id) : IQuery<GetPurchaseContentResult>;

public record GetPurchaseContentResult(IReadOnlyList<PurchaseContentDto> Content);

public class GetPurchaseContentHandler(
    IReadRepository<Purchase, Guid> repository
) : IQueryHandler<GetPurchaseContentQuery, GetPurchaseContentResult>
{
    public async Task<GetPurchaseContentResult> Handle(
        GetPurchaseContentQuery request,
        CancellationToken cancellationToken)
    {
        var result = await repository
                         .Query
                         .Where(x => x.Id == request.Id)
                         .AsExpandable()
                         .Select(x => x.Contents.Select(z => PurchaseProjections.ToContentDto.Invoke(z)))
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new PurchaseNotFoundException(request.Id);
        return new GetPurchaseContentResult(result.ToList());
    }
}