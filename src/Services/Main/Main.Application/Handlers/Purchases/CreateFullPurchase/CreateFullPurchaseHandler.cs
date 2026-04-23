using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Articles;
using Contracts.Purchase;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.Purchases.UpsertLogisticsToPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Entities;
using Main.Entities.Purchase;
using Main.Entities.Transaction;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;
using ContractPurchase = Contracts.Models.Purchase.Purchase;

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
    string? StorageFrom) : ICommand;

public class CreateFullPurchaseHandler(
    IMediator mediator,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork,
    IPurchaseService purchaseService) : ICommandHandler<CreateFullPurchaseCommand>
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

        var (usedRoute, deliveryCost) = await purchaseService.CalculateDeliveryCost(content, request.StorageFrom!,
            storageName, x => x.CalculateLogistics, cancellationToken);

        Transaction? logisticsTransaction = null;
        if (usedRoute.CarrierId != null)
            logisticsTransaction = await CreateTransaction(Global.SystemId, usedRoute.CarrierId.Value,
                deliveryCost.TotalCost, TransactionStatus.Logistics, deliveryCost.CurrencyId, whoCreated, dateTime,
                cancellationToken);

        await UpsertPurchaseLogistics(purchase.Id, usedRoute.Id, logisticsTransaction?.Id,
            deliveryCost.MinimalPriceApplied, cancellationToken);

        await purchaseService.AddLogisticsContentToPurchase(content, purchase.PurchaseContents, deliveryCost,
            cancellationToken);

        await publishEndpoint.Publish(new ArticleBuyPricesChangedEvent
        {
            ArticleIds = content.Select(x => x.ArticleId)
        }, cancellationToken);

        await publishEndpoint.Publish(new PurchaseCreatedEvent
        {
            Purchase = purchase.Adapt<ContractPurchase>()
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async Task UpsertPurchaseLogistics(
        string purchaseId,
        Guid routeId,
        Guid? transactionId,
        bool minimumPriceApplied,
        CancellationToken cancellationToken)
    {
        var command = new UpsertPurchaseLogisticsCommand(purchaseId, routeId, transactionId, minimumPriceApplied);
        await mediator.Send(command, cancellationToken);
    }

    private async Task<Transaction> CreateTransaction(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        TransactionStatus status,
        int currencyId,
        Guid whoCreatedUserId,
        DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(senderId, receiverId, amount, currencyId, whoCreatedUserId, dateTime,
            status);
        return (await mediator.Send(command, cancellationToken)).Transaction;
    }

    private async Task<Purchase> CreatePurchase(
        List<NewPurchaseContentDto> content,
        List<StorageContentDto> storageContents,
        int currencyId,
        string? comment,
        Guid supplierId,
        Guid whoCreated,
        Guid transactionId,
        string storageName,
        DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        List<(NewPurchaseContentDto, int?)> pContent = content
            .Select((t, i) => (t, (int?)storageContents[i].Id))
            .ToList();

        var command = new CreatePurchaseCommand(pContent, currencyId, comment, whoCreated, transactionId,
            storageName, supplierId, dateTime);
        var result = await mediator.Send(command, cancellationToken);
        return result.Purchase;
    }

    private async Task<List<StorageContentDto>> AddContentToStorage(
        List<NewPurchaseContentDto> content,
        string storageName,
        Guid userId,
        int currencyId,
        DateTime purchaseDate,
        CancellationToken cancellationToken = default)
    {
        var storageContents = content.Select(x =>
        {
            var temp = x.Adapt<NewStorageContentDto>();
            temp.CurrencyId = currencyId;
            temp.PurchaseDate = purchaseDate;
            return temp;
        });
        var command = new AddContentCommand(storageContents, storageName, userId, StorageMovementType.Purchase, false);
        var result = await mediator.Send(command, cancellationToken);
        return result.StorageContents;
    }
}