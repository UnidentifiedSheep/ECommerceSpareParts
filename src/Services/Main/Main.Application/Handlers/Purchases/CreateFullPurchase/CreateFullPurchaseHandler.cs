using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;
using Main.Application.Handlers.Purchases.AddLogisticsToPurchase;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Entities;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record CreateFullPurchaseCommand(
    Guid CreatedUserId,
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom,
    int? LogisticsCurrencyId) : ICommand;

public class CreateFullPurchaseHandler(IMediator mediator) : ICommandHandler<CreateFullPurchaseCommand>
{
    public async Task<Unit> Handle(CreateFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        var content = request.PurchaseContent.ToList();
        var supplierId = request.SupplierId;
        var whoCreated = request.CreatedUserId;
        var currencyId = request.CurrencyId;
        var storageName = request.StorageName;
        var payedSum = request.PayedSum ?? 0;
        var dateTime = request.PurchaseDate;
        var totalSum = content.GetTotalSum();

        var transaction = await CreateTransaction(supplierId, Global.SystemId, totalSum, TransactionStatus.Purchase,
            currencyId, whoCreated, dateTime, cancellationToken);
        
        var storageContents = await AddContentToStorage(content, storageName, whoCreated, 
            currencyId, dateTime, cancellationToken);

        var purchase = await CreatePurchase(content, storageContents, currencyId, request.Comment, supplierId, 
            whoCreated, transaction.Id, storageName, dateTime, cancellationToken);


        if (payedSum > 0)
            await CreateTransaction(Global.SystemId, supplierId, payedSum, TransactionStatus.Normal, currencyId,
                whoCreated, dateTime, cancellationToken);

        if (!request.WithLogistics) return Unit.Value;
        
        var (usedRoute, deliveryCost) = await CalculateDeliveryCost(request.StorageFrom!, storageName, content,
            request.LogisticsCurrencyId!.Value, cancellationToken);
        
        Transaction? logisticsTransaction = null;
        if (usedRoute.CarrierId != null)
        {
            logisticsTransaction = await CreateTransaction(Global.SystemId, usedRoute.CarrierId.Value, 
                deliveryCost.TotalCost, TransactionStatus.Logistics, deliveryCost.CurrencyId, whoCreated, dateTime, 
                cancellationToken);
        }

        await AddLogisticsToPurchase(purchase.Id, usedRoute.Id, logisticsTransaction?.Id,
            deliveryCost.MinimalPriceApplied, cancellationToken);
            
        await AddLogisticsContentToPurchase(content, purchase.PurchaseContents, 
            deliveryCost, cancellationToken);

        return Unit.Value;
    }

    private async Task AddLogisticsContentToPurchase(List<NewPurchaseContentDto> contentDtos, 
        IEnumerable<PurchaseContent> contents, DeliveryCostDto costs, CancellationToken cancellationToken)
    {
        List<PurchaseContentLogisticDto> contentLogistics = [];
        var contentsList = contents.ToList();

        int costsIndex = 0;
        for (int i = 0; i < contentDtos.Count; i++)
        {
            var dto = contentDtos[i];
            if (!dto.CalculateLogistics) continue;
            
            var content = contentsList[i];
            var deliveryItemInfo = costs.Items[costsIndex];
            
            contentLogistics.Add(new PurchaseContentLogisticDto
            {
                PurchaseContentId = content.Id,
                WeightKg = deliveryItemInfo.Weight.ToKg(deliveryItemInfo.WeightUnit),
                AreaM3 = deliveryItemInfo.AreaM3
            });
            costsIndex++;
        }

        var command = new AddContentLogisticsToPurchaseCommand(contentLogistics);
        await mediator.Send(command, cancellationToken);
    }
    private async Task AddLogisticsToPurchase(string purchaseId, Guid routeId, Guid? transactionId, 
        bool minimumPriceApplied, CancellationToken cancellationToken)
    {
        var command = new AddLogisticsToPurchaseCommand(purchaseId, routeId, transactionId, minimumPriceApplied);
        await mediator.Send(command, cancellationToken);
    }

    private async Task<(StorageRouteDto usedRoute, DeliveryCostDto deliveryCost)> CalculateDeliveryCost(string storageFrom, 
        string storageTo, List<NewPurchaseContentDto> content, int logisticsCurrencyId, CancellationToken token)
    {
        var items = content.Where(x => x.CalculateLogistics)
            .Adapt<List<LogisticsItemDto>>();

        var query = new CalculateDeliveryCostQuery(storageFrom, storageTo, logisticsCurrencyId, items);
        var result = await mediator.Send(query, token);
        return (result.Route, result.DeliveryCost);
    }
    private async Task<Transaction> CreateTransaction(Guid senderId, Guid receiverId, decimal amount,
        TransactionStatus status, int currencyId,
        Guid whoCreatedUserId, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(senderId, receiverId, amount, currencyId, whoCreatedUserId, dateTime,
            status);
        return (await mediator.Send(command, cancellationToken)).Transaction;
    }

    private async Task<Purchase> CreatePurchase(List<NewPurchaseContentDto> content, 
        List<StorageContentDto> storageContents, int currencyId, string? comment,
        Guid supplierId, Guid whoCreated, Guid transactionId, string storageName, DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < storageContents.Count; i++)
            content[i].StorageContentId = storageContents[i].Id;
        
        var command = new CreatePurchaseCommand(content, currencyId, comment, whoCreated, transactionId, 
            storageName, supplierId, dateTime);
        var result = await mediator.Send(command, cancellationToken);
        return result.Purchase;
    }

    private async Task<List<StorageContentDto>> AddContentToStorage(List<NewPurchaseContentDto> content, string storageName, Guid userId,
        int currencyId, DateTime purchaseDate, CancellationToken cancellationToken = default)
    {
        var storageContents = content.Select(x =>
        {
            var temp = x.Adapt<NewStorageContentDto>();
            temp.CurrencyId = currencyId;
            temp.PurchaseDate = purchaseDate;
            return temp;
        });
        var command = new AddContentCommand(storageContents, storageName, userId, StorageMovementType.Purchase);
        var result = await mediator.Send(command, cancellationToken);
        return result.StorageContents;
    }
}