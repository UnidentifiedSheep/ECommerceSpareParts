using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Command;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Purchase;
using Exceptions.Exceptions.Purchase;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Application.Handlers.Purchases.ClearPurchaseLogistics;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Entities;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

using ContractPurchase = Contracts.Models.Purchase.Purchase;

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

public class EditFullPurchaseHandler(
    IMediator mediator, 
    IPurchaseRepository purchaseRepository, 
    IUnitOfWork unitOfWork, 
    IPublishEndpoint publishEndpoint,
    IPurchaseService purchaseService) : ICommandHandler<EditFullPurchaseCommand>
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

        await EditTransaction(purchase.TransactionId, currencyId, totalSum, dateTime, TransactionStatus.Purchase, cancellationToken);

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

        var (route, deliveryCost) = await purchaseService.CalculateDeliveryCost(content, request.StorageFrom!,
            purchase.Storage, x => x.CalculateLogistics, cancellationToken);

        var (purchaseLogistic, contents) = 
            await ClearOldLogisticsData(purchaseId, CommandPresets.WithOutSaveChanges, cancellationToken);

        Transaction? logisticsTransaction = await purchaseService.UpsertLogisticsTransaction(purchaseLogistic, route,
            deliveryCost, whoUpdated, dateTime, cancellationToken);
        
        await UpsertPurchaseLogistics(purchaseId, route.Id, logisticsTransaction?.Id, deliveryCost.MinimalPriceApplied,
            cancellationToken);

        await purchaseService.AddLogisticsContentToPurchase(content, contents, deliveryCost, cancellationToken);

        await publishEndpoint.Publish(new PurchaseUpdateEvent()
        {
            Purchase = purchase.Adapt<ContractPurchase>()
        }, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }

    private async Task<ClearPurchaseLogisticsResult> ClearOldLogisticsData(
        string purchaseId, CommandOptions options, CancellationToken cancellationToken)
    {
        return await mediator.Send(new ClearPurchaseLogisticsCommand(purchaseId, options), cancellationToken);
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

    private async Task EditTransaction(Guid transactionId, int currencyId, decimal amount, DateTime dateTime,
        TransactionStatus status, CancellationToken cancellationToken = default)
    {
        var command =
            new EditTransactionCommand(transactionId, currencyId, amount, status, dateTime);
        await mediator.Send(command, cancellationToken);
    }

    private async Task DeleteTransaction(Guid transactionId, Guid whoDeleted, CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, whoDeleted, true);
        await mediator.Send(command, cancellationToken);
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