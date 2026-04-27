using Main.Application.Dtos.Amw.Logistics;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Storage;
using Main.Entities.Balance;
using Main.Entities.Purchase;

namespace Main.Abstractions.Interfaces.Services;

public interface IPurchaseService
{
    Task AddLogisticsContentToPurchase(
        List<EditPurchaseDto> contentDtos,
        IEnumerable<PurchaseContent> contents,
        DeliveryCostDto costs,
        CancellationToken cancellationToken);

    Task AddLogisticsContentToPurchase(
        List<NewPurchaseContentDto> contentDtos,
        IEnumerable<PurchaseContent> contents,
        DeliveryCostDto costs,
        CancellationToken cancellationToken);

    Task<(StorageRouteDto Route, DeliveryCostDto Cost)> CalculateDeliveryCost<TDto>(
        IEnumerable<TDto> content,
        string storageFrom,
        string storageTo,
        Func<TDto, bool> shouldCalculate,
        CancellationToken token);

    Task<Transaction?> UpsertLogisticsTransaction(
        PurchaseLogistic? purchaseLogistic,
        StorageRouteDto route,
        DeliveryCostDto deliveryCost,
        Guid whoUpdated,
        DateTime dateTime,
        CancellationToken cancellationToken);
}