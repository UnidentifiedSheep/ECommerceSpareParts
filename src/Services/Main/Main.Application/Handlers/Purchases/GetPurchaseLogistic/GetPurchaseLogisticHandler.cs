using Application.Common.Interfaces;
using Main.Application.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Purchase;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseLogistic;

public record GetPurchaseLogisticQuery(string PurchaseId) : IQuery<GetPurchaseLogisticResult>;

public record GetPurchaseLogisticResult(PurchaseLogisticDto PurchaseLogistic);

public class GetPurchaseLogisticHandler()
    : IQueryHandler<GetPurchaseLogisticQuery, GetPurchaseLogisticResult>
{
    public async Task<GetPurchaseLogisticResult> Handle(
        GetPurchaseLogisticQuery request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}