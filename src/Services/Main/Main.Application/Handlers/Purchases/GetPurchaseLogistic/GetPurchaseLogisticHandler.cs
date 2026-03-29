using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Exceptions.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Entities;
using Mapster;

namespace Main.Application.Handlers.Purchases.GetPurchaseLogistic;

public record GetPurchaseLogisticQuery(string PurchaseId) : IQuery<GetPurchaseLogisticResult>;

public record GetPurchaseLogisticResult(PurchaseLogisticDto PurchaseLogistic);

public class GetPurchaseLogisticHandler(IPurchaseLogisticsRepository purchaseLogisticsRepository)
    : IQueryHandler<GetPurchaseLogisticQuery, GetPurchaseLogisticResult>
{
    public async Task<GetPurchaseLogisticResult> Handle(
        GetPurchaseLogisticQuery request,
        CancellationToken cancellationToken)
    {
        var options = new QueryOptions<PurchaseLogistic, string>() { Data = request.PurchaseId }
            .WithTracking(false)
            .WithInclude(x => x.Currency);
        var logistic = await purchaseLogisticsRepository
                           .GetPurchaseLogistics(options, cancellationToken)
                       ?? throw new PurchaseLogisticNotFoundException(request.PurchaseId);

        return new GetPurchaseLogisticResult(logistic.Adapt<PurchaseLogisticDto>());
    }
}