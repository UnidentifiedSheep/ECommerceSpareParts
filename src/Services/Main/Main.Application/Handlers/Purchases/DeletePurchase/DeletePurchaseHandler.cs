using System.Data;
using Abstractions.Interfaces.Persistence;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Purchase;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;

namespace Main.Application.Handlers.Purchases.DeletePurchase;

[AutoSave]
[Transactional(
    IsolationLevel.Serializable,
    20,
    2)]
public record DeletePurchaseCommand(Guid PurchaseId) : ICommand<Unit>;

public class DeletePurchaseHandler(
    IUnitOfWork unitOfWork,
    IRepository<Purchase, Guid> repository,
    IIntegrationEventScope interfaceScope,
    ISender sender
)
    : ICommandHandler<DeletePurchaseCommand, Unit>
{
    public async Task<Unit> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchase = await GetPurchase(request.PurchaseId, cancellationToken);
        await SubtractStock(purchase, cancellationToken);
        await RevertTransactions(purchase, cancellationToken);

        unitOfWork.Remove(purchase);

        interfaceScope.Add(
            new PurchaseDeleteEvent
            {
                PurchaseId = request.PurchaseId
            });

        return Unit.Value;
    }

    private async Task<Purchase> GetPurchase(Guid id, CancellationToken cancellationToken)
    {
        return await repository.EnsureExistForUpdateAsync(
            id,
            key => new PurchaseNotFoundException(key),
            Criteria<Purchase>.New()
                .Include(x => x.PurchaseLogistic)
                .Include(x => x.Contents),
            cancellationToken);
    }

    private async Task SubtractStock(
        Purchase purchase,
        CancellationToken token)
    {
        var items = purchase.Contents
            .Select(x => new SubtractStorageContentItem(x.StorageContentId, x.Count));
        var command = new SubtractStorageContentsCommand(items, StorageMovementType.PurchaseDeletion);
        await sender.Send(command, token);
    }

    private async Task RevertTransactions(
        Purchase purchase,
        CancellationToken token)
    {
        await sender.Send(
            new ReverseTransactionCommand(
                purchase.TransactionId,
                TransactionReversalMode.System,
                true),
            token);

        if (purchase.PurchaseLogistic?.TransactionId != null)
            await sender.Send(
                new ReverseTransactionCommand(
                    purchase.PurchaseLogistic.TransactionId.Value,
                    TransactionReversalMode.System,
                    true),
                token);
    }
}