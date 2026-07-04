using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetFullPurchase;

public record GetFullPurchaseQuery(Guid PurchaseId) : IQuery<GetFullPurchaseResult>;

public record GetFullPurchaseResult(PurchaseDto Purchase, IEnumerable<PurchaseContentDto> Contents);

public class GetFullPurchaseHandler(
    IReadRepository<Purchase, Guid> readRepository
) : IQueryHandler<GetFullPurchaseQuery, GetFullPurchaseResult>
{
    public async Task<GetFullPurchaseResult> Handle(
        GetFullPurchaseQuery request,
        CancellationToken cancellationToken)
    {
        var result = await readRepository
                         .Query
                         .Where(x => x.Id == request.PurchaseId)
                         .AsExpandable()
                         .Select(x => new
                         {
                             purchase = PurchaseProjections.ToPurchaseDto.Invoke(x),
                             contents = x.Contents.Select(z => PurchaseProjections.ToContentDto.Invoke(z))
                         })
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new PurchaseNotFoundException(request.PurchaseId);

        return new GetFullPurchaseResult(result.purchase, result.contents);
    }
}