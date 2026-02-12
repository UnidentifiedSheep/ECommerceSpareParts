using System.Data;
using Application.Common.Interfaces;
using Attributes;
using Exceptions.Exceptions.Purchase;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Abstractions.Dtos.Amw.Storage;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Purchases.EditFullPurchase;

[Transactional(IsolationLevel.Serializable, 20, 2)]
public record EditFullPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    string PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    Guid UpdatedUserId) : ICommand;

public class EditFullPurchaseHandler(IMediator mediator, IPurchaseRepository purchaseRepository)
    : ICommandHandler<EditFullPurchaseCommand>
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

        var purchase = await purchaseRepository.GetPurchaseForUpdate(purchaseId, true, cancellationToken)
                       ?? throw new PurchaseNotFoundException(purchaseId);

        var editedCounts = await EditPurchase(content, purchaseId, currencyId, comment,
            whoUpdated, dateTime, cancellationToken);

        await EditTransaction(purchase.TransactionId, currencyId, totalSum, dateTime, cancellationToken);

        await AddOrRemoveContentToStorage(editedCounts, purchase.Storage, currencyId, whoUpdated, cancellationToken);

        return Unit.Value;
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

    private async Task AddOrRemoveContentToStorage(Dictionary<int, Dictionary<decimal, int>> values, string storageName,
        int currencyId, Guid whoUpdated, CancellationToken cancellationToken = default)
    {
        var returnedToStorage = new List<NewStorageContentDto>();
        var takenFromStorage = new Dictionary<int, int>();

        foreach (var (articleId, pricesList) in values)
        foreach (var (price, count) in pricesList)
            if (count > 0)
                returnedToStorage.Add(new NewStorageContentDto
                {
                    ArticleId = articleId,
                    BuyPrice = price,
                    Count = count,
                    CurrencyId = currencyId
                });
            else if (count < 0)
                takenFromStorage[articleId] = takenFromStorage.GetValueOrDefault(articleId) + -count;

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