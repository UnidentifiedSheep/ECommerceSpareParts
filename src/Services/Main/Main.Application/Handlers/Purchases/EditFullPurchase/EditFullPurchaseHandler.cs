using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Purchase;
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
    private static readonly QueryOptions<Purchase> PurchaseOptions = new QueryOptions<Purchase>()
        .WithForUpdate()
        .WithTracking()
        .WithInclude(x => x.PurchaseLogistic);
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

        if (!request.WithLogistics) return Unit.Value;

        var (route, deliveryCost) =
            await CalculateDeliveryCost(request.StorageFrom!, purchase.Storage, content, cancellationToken);
        
        if (purchase.PurchaseLogistic != null)
        {
            Guid prevRouteId = purchase.PurchaseLogistic.RouteId;
            Guid? deliveryTransactionId = purchase.PurchaseLogistic.TransactionId;
            
            if (prevRouteId != route.Id && deliveryTransactionId != null)
                await DeleteTransaction(deliveryTransactionId.Value, whoUpdated, cancellationToken);
            //We remove purchase logistics, to be able to add new later.
            //TODO: make upsert in future
            unitOfWork.Remove(purchase.PurchaseLogistic);
        }

        Transaction? logisticsTransaction = route.CarrierId != null
            ? await CreateTransaction(Global.SystemId, route.CarrierId.Value,
                deliveryCost.TotalCost, TransactionStatus.Logistics, deliveryCost.CurrencyId, whoUpdated, dateTime,
                cancellationToken)
            : null;
        
        var command = new UpsertLogisticsToPurchaseCommand(
            purchaseId, 
            route.Id, 
            logisticsTransaction?.Id, 
            deliveryCost.MinimalPriceApplied);
        await mediator.Send(command, cancellationToken);
        
        return Unit.Value;
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

    private async Task<Dictionary<int, Dictionary<decimal, int>>> EditPurchase(List<EditPurchaseDto> contentList,
        string purchaseId, int currencyId,
        string? comment, Guid whoUpdated, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var command = new EditPurchaseCommand(contentList, purchaseId, currencyId, comment, whoUpdated, dateTime);
        return (await mediator.Send(command, cancellationToken)).EditedCounts;
    }

    private async Task EditTransaction(Guid transactionId, int currencyId, decimal amount, DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        var command =
            new EditTransactionCommand(transactionId, currencyId, amount, TransactionStatus.Purchase, dateTime);
        await mediator.Send(command, cancellationToken);
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