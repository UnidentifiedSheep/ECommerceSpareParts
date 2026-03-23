using System.Data;
using Abstractions.Interfaces.Services;
using Abstractions.Models.Repository;
using Application.Common.Interfaces;
using Attributes;
using Contracts.Purchase;
using Exceptions.Exceptions.Purchase;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Entities;
using Main.Enums;
using Mapster;
using MassTransit;
using MediatR;

using ContractPurchase = Contracts.Models.Purchase.Purchase;

namespace Main.Application.Handlers.Purchases.DeleteFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullPurchaseCommand(string PurchaseId, Guid WhoDeleted) : ICommand;

public class DeleteFullPurchaseHandler(
    IPurchaseRepository purchaseRepository, 
    IPurchaseLogisticsRepository purchaseLogisticsRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IMediator mediator) : ICommandHandler<DeleteFullPurchaseCommand>
{
    private static readonly QueryOptions<PurchaseContent> ContentOptions = new QueryOptions<PurchaseContent>()
        .WithTracking()
        .WithForUpdate();
    public async Task<Unit> Handle(DeleteFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var purchase = await purchaseRepository.GetPurchase(
                           purchaseId, 
                           QueryPresets.TrackForUpdate, 
                           cancellationToken)
                       ?? throw new PurchaseNotFoundException(purchaseId);
        var purchaseLogistics = await purchaseLogisticsRepository
            .GetPurchaseLogistics(purchaseId, QueryPresets.Track, token: cancellationToken);
        
        var purchaseContents = (await purchaseRepository.GetPurchaseContent(purchaseId,
            ContentOptions, cancellationToken)).ToList();

        await RemoveContentFromStorage(purchaseContents, request.WhoDeleted, purchase.Storage, cancellationToken);
        await DeletePurchase(purchaseId, cancellationToken);
        await DeleteTransaction(purchase.TransactionId, request.WhoDeleted, cancellationToken);

        if (purchaseLogistics?.TransactionId != null)
            await DeleteTransaction(purchaseLogistics.TransactionId.Value, request.WhoDeleted, cancellationToken);
        
        await publishEndpoint.Publish(new PurchaseDeleteEvent
        {
            Purchase = purchase.Adapt<ContractPurchase>()
        }, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }

    private async Task DeleteTransaction(Guid transactionId, Guid whoDeleted,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, whoDeleted, true);
        await mediator.Send(command, cancellationToken);
    }

    private async Task DeletePurchase(string purchaseId, CancellationToken cancellationToken = default)
    {
        var command = new DeletePurchaseCommand(purchaseId);
        await mediator.Send(command, cancellationToken);
    }

    private async Task RemoveContentFromStorage(List<PurchaseContent> purchaseContents, Guid whoRemoved,
        string storageName,
        CancellationToken cancellationToken = default)
    {
        var toRemoveFromStorage = purchaseContents
            .GroupBy(pc => pc.ArticleId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Count)
            );
        var command = new RemoveContentCommand(toRemoveFromStorage, whoRemoved, storageName, false,
            StorageMovementType.PurchaseDeletion);
        await mediator.Send(command, cancellationToken);
    }
}