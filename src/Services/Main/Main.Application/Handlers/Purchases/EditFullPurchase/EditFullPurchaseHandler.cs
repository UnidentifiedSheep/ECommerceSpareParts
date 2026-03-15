using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Command;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Purchase;
using Extensions;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Handlers.Purchases.AddContentLogisticsToPurchase;
using Main.Application.Handlers.Purchases.ClearPurchaseLogistics;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Entities;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Purchases.EditFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditFullPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    string PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    Guid UpdatedUserId,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class EditFullPurchaseHandler(IMediator mediator, IPurchaseRepository purchaseRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<EditFullPurchaseCommand>
{
    public async Task<Unit> Handle(EditFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        var dateTime = request.PurchaseDateTime;
        var content = request.Content.ToList();
        var purchaseId = request.PurchaseId;
        var currencyId = request.CurrencyId;
        var comment = request.Comment;
        var whoUpdated = request.UpdatedUserId;
        var totalSum = content.GetTotalSum();

        var purchase = await purchaseRepository.GetPurchase(
                           purchaseId, 
                           QueryPresets.TrackForUpdate, 
                           cancellationToken) ?? throw new PurchaseNotFoundException(purchaseId);
        
        var editedCounts = await EditPurchase(content, purchaseId, currencyId, comment,
            whoUpdated, dateTime, cancellationToken);

        await EditTransaction(purchase.TransactionId, currencyId, totalSum, dateTime, cancellationToken);

        await AddOrRemoveContentToStorage(editedCounts, purchase.Storage, currencyId, whoUpdated, cancellationToken);
        
        if (!request.WithLogistics)
        {
            //delete old delivery transaction
            Guid? deliveryTransactionId = purchase.PurchaseLogistic?.TransactionId;
            if (deliveryTransactionId != null)
                await DeleteTransaction(deliveryTransactionId.Value, whoUpdated, cancellationToken);
            await ClearOldLogisticsData(purchaseId, CommandPresets.WithSaveChanges, cancellationToken);
            return Unit.Value;
        }

        var (route, deliveryCost) =
            await CalculateDeliveryCost(request.StorageFrom!, purchase.Storage, content, cancellationToken);

        var (purchaseLogistic, contents) = 
            await ClearOldLogisticsData(purchaseId, CommandPresets.WithOutSaveChanges, cancellationToken);

        Transaction? logisticsTransaction = null;
        
        //rewrite, hard to read.
        if (purchaseLogistic != null)
        {
            var deliveryTransactionId = purchaseLogistic.TransactionId;
            var prevCarrierId = deliveryTransactionId == null ? null : (Guid?)purchase.Transaction.ReceiverId;

            if (deliveryTransactionId != null && prevCarrierId == route.CarrierId)
            {
                logisticsTransaction = await EditTransaction(deliveryTransactionId.Value, deliveryCost.CurrencyId, 
                    deliveryCost.TotalCost, dateTime, cancellationToken);
            }
            else
            {
                if (deliveryTransactionId != null)
                    await DeleteTransaction(deliveryTransactionId.Value, whoUpdated, cancellationToken);
                
                if (route.CarrierId != null)
                    logisticsTransaction = await CreateTransaction(Global.SystemId, route.CarrierId.Value, 
                        deliveryCost.TotalCost, TransactionStatus.Logistics, deliveryCost.CurrencyId, 
                        whoUpdated, dateTime, cancellationToken);
            }
        }
        
        await UpsertPurchaseLogistics(purchaseId, route.Id, logisticsTransaction?.Id, deliveryCost.MinimalPriceApplied,
            cancellationToken);

        await AddLogisticsContentToPurchase(content, contents, deliveryCost, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
    
    private async Task AddLogisticsContentToPurchase(List<EditPurchaseDto> contentDtos, 
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
                AreaM3 = deliveryItemInfo.AreaM3,
                Price = deliveryItemInfo.Cost
            });
            costsIndex++;
        }

        var command = new AddContentLogisticsToPurchaseCommand(contentLogistics);
        await mediator.Send(command, cancellationToken);
    }

    private async Task<ClearPurchaseLogisticsResult> ClearOldLogisticsData(
        string purchaseId, CommandOptions options, CancellationToken cancellationToken)
    {
        return await mediator.Send(new ClearPurchaseLogisticsCommand(purchaseId, options), cancellationToken);
    }
    
    private async Task<(StorageRouteDto usedRoute, DeliveryCostDto deliveryCost)> CalculateDeliveryCost(string storageFrom, 
        string storageTo, List<EditPurchaseDto> content, CancellationToken token)
    {
        var items = content.Where(x => x.CalculateLogistics)
            .Adapt<List<LogisticsItemDto>>();

        var query = new CalculateDeliveryCostQuery(storageFrom, storageTo, items);
        var result = await mediator.Send(query, token);
        return (result.Route, result.DeliveryCost);
    }

    private async Task UpsertPurchaseLogistics(string purchaseId, Guid routeId, Guid? logisticsTransactionId, 
        bool minPriceApplied, CancellationToken cancellationToken)
    {
        var command = new UpsertPurchaseLogisticsCommand(purchaseId, routeId, logisticsTransactionId, minPriceApplied);
        await mediator.Send(command, cancellationToken);
    }

    private async Task<Dictionary<int, Dictionary<decimal, int>>> EditPurchase(List<EditPurchaseDto> contentList,
        string purchaseId, int currencyId,
        string? comment, Guid whoUpdated, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var command = new EditPurchaseCommand(contentList, purchaseId, currencyId, comment, whoUpdated, dateTime);
        return (await mediator.Send(command, cancellationToken)).EditedCounts;
    }

    private async Task<Transaction> EditTransaction(Guid transactionId, int currencyId, decimal amount, DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        var command =
            new EditTransactionCommand(transactionId, currencyId, amount, TransactionStatus.Purchase, dateTime);
        return (await mediator.Send(command, cancellationToken)).Transaction;
    }

    private async Task DeleteTransaction(Guid transactionId, Guid whoDeleted, CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, whoDeleted, true);
        await mediator.Send(command, cancellationToken);
    }
    
    private async Task<Transaction> CreateTransaction(Guid senderId, Guid receiverId, decimal amount,
        TransactionStatus status, int currencyId,
        Guid whoCreatedUserId, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(senderId, receiverId, amount, currencyId, whoCreatedUserId, dateTime,
            status);
        return (await mediator.Send(command, cancellationToken)).Transaction;
    }

    private async Task AddOrRemoveContentToStorage(Dictionary<int, Dictionary<decimal, int>> values, string storageName,
        int currencyId, Guid whoUpdated, CancellationToken cancellationToken = default)
    {
        var returnedToStorage = new List<NewStorageContentDto>();
        var takenFromStorage = new Dictionary<int, int>();

        foreach (var (articleId, pricesList) in values)
        foreach (var (price, count) in pricesList)
            switch (count)
            {
                case > 0:
                    returnedToStorage.Add(new NewStorageContentDto
                    {
                        ArticleId = articleId,
                        BuyPrice = price,
                        Count = count,
                        CurrencyId = currencyId
                    });
                    break;
                case < 0:
                    takenFromStorage[articleId] = takenFromStorage.GetValueOrDefault(articleId) + -count;
                    break;
            }
        
        var returnToStorageCommand = new AddContentCommand(returnedToStorage, storageName, whoUpdated,
            StorageMovementType.PurchaseEditing);
        var takeFromStorageCommand = new RemoveContentCommand(takenFromStorage, whoUpdated, storageName, false,
            StorageMovementType.PurchaseEditing);

        if (returnedToStorage.Count > 0)
            await mediator.Send(returnToStorageCommand, cancellationToken);
        if (takenFromStorage.Count > 0)
            await mediator.Send(takeFromStorageCommand, cancellationToken);
    }
}