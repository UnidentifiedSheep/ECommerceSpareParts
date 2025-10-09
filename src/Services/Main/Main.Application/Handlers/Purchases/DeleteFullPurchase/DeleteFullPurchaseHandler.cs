using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.DbRepositories;
using Exceptions.Exceptions.Purchase;
using Main.Application.Handlers.Balance.DeleteTransaction;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.StorageContents.RemoveContent;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeleteFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record DeleteFullPurchaseCommand(string PurchaseId, Guid WhoDeleted) : ICommand;

public class DeleteFullPurchaseHandler(IPurchaseRepository purchaseRepository, IMediator mediator)
    : ICommandHandler<DeleteFullPurchaseCommand>
{
    public async Task<Unit> Handle(DeleteFullPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var purchase = await purchaseRepository.GetPurchaseForUpdate(purchaseId, true, cancellationToken)
                       ?? throw new PurchaseNotFoundException(purchaseId);
        var purchaseContents = (await purchaseRepository.GetPurchaseContentForUpdate(purchaseId,
            true, cancellationToken)).ToList();

        await RemoveContentFromStorage(purchaseContents, request.WhoDeleted, purchase.Storage, cancellationToken);
        await DeletePurchase(purchaseId, cancellationToken);
        await DeleteTransaction(purchase.TransactionId, request.WhoDeleted, cancellationToken);

        return Unit.Value;
    }

    private async Task DeleteTransaction(string transactionId, Guid whoDeleted,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteTransactionCommand(transactionId, whoDeleted, true);
        await mediator.Publish(command, cancellationToken);
    }

    private async Task DeletePurchase(string purchaseId, CancellationToken cancellationToken = default)
    {
        var command = new DeletePurchaseCommand(purchaseId);
        await mediator.Publish(command, cancellationToken);
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
        await mediator.Publish(command, cancellationToken);
    }
}