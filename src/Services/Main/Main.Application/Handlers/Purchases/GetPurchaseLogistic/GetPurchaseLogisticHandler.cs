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
    private static readonly QueryOptions<PurchaseLogistic> QueryOptions = new QueryOptions<PurchaseLogistic>()
        .WithInclude(x => x.Currency);
    public async Task<GetPurchaseLogisticResult> Handle(GetPurchaseLogisticQuery request, CancellationToken cancellationToken)
    {
        PurchaseLogistic logistic = await purchaseLogisticsRepository
            .GetPurchaseLogistics(request.PurchaseId, QueryOptions, cancellationToken)
                                    ?? throw new PurchaseLogisticNotFoundException(request.PurchaseId);
        
        return new GetPurchaseLogisticResult(logistic.Adapt<PurchaseLogisticDto>());
    }
}