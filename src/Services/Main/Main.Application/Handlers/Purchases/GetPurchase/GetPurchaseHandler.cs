using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Projections;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Main.Application.Dtos.Purchase;
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
    IReadRepository<Purchase, Guid> repository,
    IProjectionProvider<Purchase, PurchaseDto> projection
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
            .Project(projection)
            .FirstOrDefaultAsync(cancellationToken);

        return dto == null
            ? throw new PurchaseNotFoundException(request.PurchaseId ?? request.TransactionId!.Value)
            : new GetPurchaseResult(dto);
    }
}
