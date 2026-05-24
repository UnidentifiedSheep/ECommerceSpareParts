using Application.Common.Interfaces.Cqrs;
using Main.Application.Dtos.Purchase;

namespace Main.Application.Handlers.Purchases.GetPurchaseLogistic;

public record GetPurchaseLogisticQuery(string PurchaseId) : IQuery<GetPurchaseLogisticResult>;

public record GetPurchaseLogisticResult(PurchaseLogisticDto PurchaseLogistic);

public class GetPurchaseLogisticHandler : IQueryHandler<GetPurchaseLogisticQuery, GetPurchaseLogisticResult>
{
    public async Task<GetPurchaseLogisticResult> Handle(
        GetPurchaseLogisticQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}