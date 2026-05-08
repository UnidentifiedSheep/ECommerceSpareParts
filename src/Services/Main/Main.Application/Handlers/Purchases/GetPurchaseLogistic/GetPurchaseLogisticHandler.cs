using Application.Common.Interfaces;
using Main.Application.Dtos.Amw.Purchase;

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