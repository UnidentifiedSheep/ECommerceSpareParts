using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Handlers.Purchases.GetPurchase;

[Diagnostics(maxExecutionTimeMs: 30)]
public record GetPurchaseQuery(
    Guid? PurchaseId,
    Guid? TransactionId
) : IQuery<GetPurchaseResult>;

public record GetPurchaseResult(PurchaseDto Purchase);

public class GetPurchaseHandler(
    IReadRepository<Purchase, Guid> repository
) : IQueryHandler<GetPurchaseQuery, GetPurchaseResult>
{
    public async Task<GetPurchaseResult> Handle(
        GetPurchaseQuery request,
        CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var transactionId = request.TransactionId;

        var dto = await repository.Query
            .Where(x =>
                purchaseId.HasValue && x.Id == purchaseId.Value ||
                transactionId.HasValue && x.TransactionId == transactionId.Value)
            .OrderByDescending(x => purchaseId.HasValue && x.Id == purchaseId.Value)
            .AsExpandable()
            .Select(PurchaseProjections.ToPurchaseDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto == null
            ? throw new PurchaseNotFoundException(request.PurchaseId ?? request.TransactionId!.Value)
            : new GetPurchaseResult(dto);
    }
}