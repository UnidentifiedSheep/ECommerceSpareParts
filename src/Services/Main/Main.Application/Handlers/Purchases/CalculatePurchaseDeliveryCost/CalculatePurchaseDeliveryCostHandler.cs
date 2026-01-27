using Application.Common.Interfaces;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Logistics;

namespace Main.Application.Handlers.Purchases.CalculatePurchaseDeliveryCost;

public record CalculatePurchaseDeliveryCostQuery(string StorageFrom, string StorageTo, 
    IEnumerable<NewPurchaseContentDto> PurchaseContent) : IQuery<CalculatePurchaseDeliveryCostResult>;
public record CalculatePurchaseDeliveryCostResult();

public class CalculatePurchaseDeliveryCostHandler(ILogisticsCostService logisticsCostService, 
    IStorageRoutesRepository storageRoutesRepository) 
    : IQueryHandler<CalculatePurchaseDeliveryCostQuery, CalculatePurchaseDeliveryCostResult>
{
    public async Task<CalculatePurchaseDeliveryCostResult> Handle(CalculatePurchaseDeliveryCostQuery request, CancellationToken cancellationToken)
    {
        string from = request.StorageFrom;
        string to = request.StorageTo;
        var route = await storageRoutesRepository.GetStorageRouteAsync(from, to, true, 
                               false, cancellationToken, x => x.Currency) 
                           ?? throw new StorageRouteNotFound(from, to);
        return new CalculatePurchaseDeliveryCostResult();
    }
}