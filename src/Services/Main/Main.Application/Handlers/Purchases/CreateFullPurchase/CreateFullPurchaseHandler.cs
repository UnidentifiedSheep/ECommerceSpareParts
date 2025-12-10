using System.Data;
using Application.Common.Interfaces;
using Core.Attributes;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Core.Dtos.Amw.Purchase;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Entities;
using Main.Core.Enums;
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
    decimal? PayedSum) : ICommand;

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

        await CreatePurchase(content, currencyId, request.Comment, supplierId, whoCreated, transaction.Id, storageName,
            dateTime, cancellationToken);

        await AddContentToStorage(content, storageName, whoCreated, currencyId, dateTime, cancellationToken);

        if (payedSum > 0)
            await CreateTransaction(Global.SystemId, supplierId, payedSum, TransactionStatus.Normal, currencyId,
                whoCreated, dateTime, cancellationToken);
        return Unit.Value;
    }

    private async Task<Transaction> CreateTransaction(Guid senderId, Guid receiverId, decimal amount,
        TransactionStatus status, int currencyId,
        Guid whoCreatedUserId, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        var command = new CreateTransactionCommand(senderId, receiverId, amount, currencyId, whoCreatedUserId, dateTime,
            status);
        return (await mediator.Send(command, cancellationToken)).Transaction;
    }

    private async Task CreatePurchase(List<NewPurchaseContentDto> content, int currencyId, string? comment,
        Guid supplierId, Guid whoCreated, Guid transactionId, string storageName, DateTime dateTime,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePurchaseCommand(content, currencyId, comment, whoCreated, transactionId, storageName,
            supplierId, dateTime);
        await mediator.Send(command, cancellationToken);
    }

    private async Task AddContentToStorage(List<NewPurchaseContentDto> content, string storageName, Guid userId,
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
        await mediator.Send(command, cancellationToken);
    }
}