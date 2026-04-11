using Extensions;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;
using Main.Entities;
using Main.Entities.Purchase;
using Main.Entities.Transaction;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Services;

public class PurchaseService(IMediator mediator) : IPurchaseService
{
    public Task AddLogisticsContentToPurchase(
        List<EditPurchaseDto> contentDtos,
        IEnumerable<PurchaseContent> contents,
        DeliveryCostDto costs,
        CancellationToken cancellationToken)
    {
        return AddLogisticsContentToPurchaseInternal(
            contentDtos,
            contents,
            costs,
            x => x.CalculateLogistics,
            cancellationToken);
    }

    public Task AddLogisticsContentToPurchase(
        List<NewPurchaseContentDto> contentDtos,
        IEnumerable<PurchaseContent> contents,
        DeliveryCostDto costs,
        CancellationToken cancellationToken)
    {
        return AddLogisticsContentToPurchaseInternal(
            contentDtos,
            contents,
            costs,
            x => x.CalculateLogistics,
            cancellationToken);
    }

    public async Task<(StorageRouteDto Route, DeliveryCostDto Cost)> CalculateDeliveryCost<TDto>(
        IEnumerable<TDto> content,
        string storageFrom,
        string storageTo,
        Func<TDto, bool> shouldCalculate,
        CancellationToken token)
    {
        var items = content
            .Where(shouldCalculate)
            .Adapt<List<LogisticsItemDto>>();

        var query = new CalculateDeliveryCostQuery(storageFrom, storageTo, items);
        var result = await mediator.Send(query, token);

        return (result.Route, result.DeliveryCost);
    }

    public async Task<Transaction?> UpsertLogisticsTransaction(
        PurchaseLogistic? purchaseLogistic,
        StorageRouteDto route,
        DeliveryCostDto deliveryCost,
        Guid whoUpdated,
        DateTime dateTime,
        CancellationToken cancellationToken)
    {
        if (purchaseLogistic == null) return null;

        var deliveryTransactionId = purchaseLogistic.TransactionId;
        var prevCarrierId = deliveryTransactionId == null
            ? null
            : (Guid?)purchaseLogistic.Transaction.ReceiverId;

        if (deliveryTransactionId != null && prevCarrierId == route.CarrierId)
            return (await mediator.Send(new EditTransactionCommand(deliveryTransactionId.Value, deliveryCost.CurrencyId,
                deliveryCost.TotalCost, TransactionStatus.Logistics, dateTime), cancellationToken)).Transaction;


        if (deliveryTransactionId != null)
            await mediator.Send(new DeleteTransactionCommand(deliveryTransactionId.Value, whoUpdated, true),
                cancellationToken);

        if (route.CarrierId == null) return null;

        return (await mediator.Send(new CreateTransactionCommand(Global.SystemId, route.CarrierId.Value,
            deliveryCost.TotalCost, deliveryCost.CurrencyId, whoUpdated, dateTime,
            TransactionStatus.Logistics), cancellationToken)).Transaction;
    }

    private async Task AddLogisticsContentToPurchaseInternal<TDto>(
        List<TDto> contentDtos,
        IEnumerable<PurchaseContent> contents,
        DeliveryCostDto costs,
        Func<TDto, bool> shouldCalculate,
        CancellationToken cancellationToken)
    {
        List<PurchaseContentLogisticDto> contentLogistics = [];
        var contentsList = contents.ToList();

        var costsIndex = 0;

        for (var i = 0; i < contentDtos.Count; i++)
        {
            var dto = contentDtos[i];
            if (!shouldCalculate(dto))
                continue;

            var content = contentsList[i];
            var deliveryItemInfo = costs.Items[costsIndex];

            contentLogistics.Add(new PurchaseContentLogisticDto
            {
                PurchaseContentId = content.Id,
                WeightKg = deliveryItemInfo.Weight.ToKg(deliveryItemInfo.WeightUnit),
                AreaM3 = deliveryItemInfo.AreaM3,
                Price = deliveryItemInfo.Cost
            });

            costsIndex++;
        }

        var command = new AddContentLogisticsToPurchaseCommand(contentLogistics);
        await mediator.Send(command, cancellationToken);
    }
}