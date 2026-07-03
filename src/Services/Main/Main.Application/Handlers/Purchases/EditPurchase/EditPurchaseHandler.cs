using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Purchase;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Models;
using Main.Entities.Balance;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.Extensions.Options;

namespace Main.Application.Handlers.Purchases.EditPurchase;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record EditPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    Guid PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    bool WithLogistics,
    string? StorageFrom
) : ICommand;

public class EditPurchaseHandler(
    ISender sender,
    IRepository<Purchase, Guid> purchaseRepository,
    IRepository<StorageOwner, (string, Guid)> storageOwnerRepository,
    IOptions<SystemOptions> systemOptions,
    IStorageContentRepository storageContentRepository,
    ICurrencyConverter currencyConverter,
    IPurchaseLogisticsService purchaseLogisticsService,
    IIntegrationEventScope integrationEventScope,
    IUnitOfWork unitOfWork
) : ICommandHandler<EditPurchaseCommand>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var purchase = await GetPurchase(purchaseId, cancellationToken);
        var contentDtos = request.Content.ToList();

        await EnsureStorageOwner(
            purchase,
            request,
            cancellationToken);
        await ReversePurchaseTransactions(purchase, cancellationToken);

        var totalSum = contentDtos.Sum(x => x.Price * x.Count);
        var purchaseTransaction = await CreateTransaction(
            purchase.SupplierId,
            GetSystemUserId(),
            totalSum,
            request.CurrencyId,
            request.PurchaseDateTime,
            cancellationToken);

        purchase.SetTransactionId(purchaseTransaction.Id);
        purchase.SetCurrencyId(request.CurrencyId);
        purchase.SetPurchaseDate(request.PurchaseDateTime);
        purchase.SetComment(request.Comment);

        await UpdateContents(
            purchase,
            contentDtos,
            request,
            cancellationToken);
        await UpdateLogistics(
            purchase,
            contentDtos,
            request,
            cancellationToken);

        integrationEventScope.Add(
            new PurchaseUpdateEvent
            {
                PurchaseId = purchase.Id
            });

        return Unit.Value;
    }

    private Guid GetSystemUserId() { return systemOptions.Value.SystemId; }

    private Task<Purchase> GetPurchase(Guid purchaseId, CancellationToken cancellationToken)
    {
        return purchaseRepository.EnsureExistForUpdateAsync(
            purchaseId,
            id => new PurchaseNotFoundException(id),
            Criteria<Purchase>.New()
                .Include(x => x.Contents)
                .Include(x => x.PurchaseLogistic),
            cancellationToken);
    }

    private async Task EnsureStorageOwner(
        Purchase purchase,
        EditPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        if (request is not { WithLogistics: true, StorageFrom: not null }) return;

        await storageOwnerRepository.EnsureExistAsync(
            (request.StorageFrom, purchase.SupplierId),
            _ => new StorageOwnerNotFoundException(purchase.SupplierId, request.StorageFrom),
            ct: cancellationToken);
    }

    private async Task ReversePurchaseTransactions(
        Purchase purchase,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ReverseTransactionCommand(
                purchase.TransactionId,
                TransactionReversalMode.System,
                true),
            cancellationToken);

        if (purchase.PurchaseLogistic?.TransactionId is { } logisticsTransactionId)
            await sender.Send(
                new ReverseTransactionCommand(
                    logisticsTransactionId,
                    TransactionReversalMode.System,
                    true),
                cancellationToken);
    }

    private async Task<Transaction> CreateTransaction(
        Guid senderId,
        Guid receiverId,
        decimal amount,
        int currencyId,
        DateTime transactionDateTime,
        CancellationToken cancellationToken)
    {
        return (await sender.Send(
                new CreateTransactionCommand(
                    senderId,
                    receiverId,
                    amount,
                    currencyId,
                    transactionDateTime,
                    TransactionSourceType.Purchase,
                    TransactionCreationMode.System),
                cancellationToken))
            .Transaction;
    }

    private async Task UpdateContents(
        Purchase purchase,
        IReadOnlyList<EditPurchaseDto> contentDtos,
        EditPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var existingById = purchase.Contents.ToDictionary(x => x.Id);
        var requestedIds = contentDtos
            .Where(x => x.Id is not null)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        requestedIds.EnsureAllExists(
            existingById.Keys,
            ids => new PurchaseContentNotFoundException(ids[0]));

        var storageContentIds = purchase.Contents
            .Select(x => x.StorageContentId)
            .ToHashSet();

        var storageContents = storageContentIds.Count == 0
            ? new Dictionary<int, StorageContent>()
            : await storageContentRepository.EnsureExistsForUpdateAsync(
                storageContentIds,
                ids => new StorageContentNotFoundException(ids[0]),
                cancellationToken);

        var deletionSubtractions = new List<SubtractStorageContentItem>();
        var editingSubtractions = new List<SubtractStorageContentItem>();
        var editingRestorations = new List<RestoreContentItem>();
        var contentToAdd = new List<EditPurchaseDto>();

        foreach (var removed in purchase.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
            RemoveContent(
                purchase,
                removed,
                deletionSubtractions);

        foreach (var dto in contentDtos)
        {
            if (dto.Id is null)
            {
                contentToAdd.Add(dto);
                continue;
            }

            await UpdateContent(
                existingById[dto.Id.Value],
                dto,
                request,
                storageContents,
                editingRestorations,
                editingSubtractions,
                cancellationToken);
        }

        if (contentToAdd.Count > 0)
            await AddContent(
                purchase,
                contentToAdd,
                cancellationToken);


        if (deletionSubtractions.Count > 0)
            await sender.Send(
                new SubtractStorageContentsCommand(
                    deletionSubtractions,
                    StorageMovementType.PurchaseEditing),
                cancellationToken);

        if (editingSubtractions.Count > 0)
            await sender.Send(
                new SubtractStorageContentsCommand(
                    editingSubtractions,
                    StorageMovementType.PurchaseEditing),
                cancellationToken);

        if (editingRestorations.Count > 0)
            await sender.Send(
                new RestoreContentCommand(
                    editingRestorations,
                    StorageMovementType.PurchaseEditing),
                cancellationToken);
    }

    private void RemoveContent(
        Purchase purchase,
        PurchaseContent content,
        ICollection<SubtractStorageContentItem> subtractions)
    {
        if (content.PurchaseContentLogistic is { } logistic) unitOfWork.Remove(logistic);

        subtractions.Add(
            new SubtractStorageContentItem(
                content.StorageContentId,
                content.Count));

        purchase.RemoveContent(content);
        unitOfWork.Remove(content);
    }

    private async Task AddContent(
        Purchase purchase,
        List<EditPurchaseDto> newContents,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new AddContentCommand(
                newContents.Select(x => new NewStorageContentDto
                    {
                        BuyPrice = x.Price,
                        Count = x.Count,
                        CurrencyId = purchase.CurrencyId,
                        ProductId = x.ProductId,
                        PurchaseDate = purchase.PurchaseDatetime
                    })
                    .ToList(),
                purchase.Storage,
                StorageMovementType.PurchaseEditing),
            cancellationToken);

        if (result.StorageContents.Count != newContents.Count)
            throw new InvalidOperationException(
                "AddContentCommand returned unexpected number of storage contents.");

        for (var i = 0; i < result.StorageContents.Count; i++)
        {
            var storageContent = result.StorageContents[i];
            var newContent = newContents[i];

            if (storageContent.ProductId != newContent.ProductId ||
                storageContent.Count != newContent.Count)
                throw new InvalidOperationException("Unexpected storage content when updating purchase.");


            purchase.AddContent(
                PurchaseContent.Create(
                    storageContent.ProductId,
                    storageContent.Count,
                    storageContent.BuyPrice,
                    storageContent.Id,
                    newContent.Comment));
        }
    }

    private async Task UpdateContent(
        PurchaseContent content,
        EditPurchaseDto dto,
        EditPurchaseCommand request,
        Dictionary<int, StorageContent> storageContents,
        List<RestoreContentItem> restorations,
        List<SubtractStorageContentItem> subtractions,
        CancellationToken cancellationToken)
    {
        if (content.ProductId != dto.ProductId) throw new ArticleDoesntMatchContentException(content.Id);

        if (!storageContents.TryGetValue(content.StorageContentId, out var storageContent))
            throw new StorageContentNotFoundException(content.StorageContentId);

        var lotIsUntouched = storageContent.Count == content.Count;
        var countDelta = dto.Count - content.Count;
        if (countDelta < 0)
            subtractions.Add(
                new SubtractStorageContentItem(
                    content.StorageContentId,
                    -countDelta));
        else if (countDelta > 0)
            restorations.Add(
                new RestoreContentItem(
                    content.StorageContentId,
                    content.ProductId,
                    request.CurrencyId,
                    dto.Price,
                    countDelta));

        content.SetCount(dto.Count);
        content.SetPrice(dto.Price);
        content.SetComment(dto.Comment);

        if (!lotIsUntouched) return;

        await UpdateStorageContentMetadata(
            storageContent,
            dto,
            request,
            cancellationToken);
    }

    private async Task UpdateStorageContentMetadata(
        StorageContent storageContent,
        EditPurchaseDto dto,
        EditPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        storageContent.SetCurrencyId(request.CurrencyId);
        storageContent.SetPurchaseDate(request.PurchaseDateTime);
        storageContent.SetBuyPrice(
            dto.Price,
            await currencyConverter.ConvertToBaseAsync(
                dto.Price,
                request.CurrencyId,
                cancellationToken));
    }

    private async Task UpdateLogistics(
        Purchase purchase,
        IReadOnlyList<EditPurchaseDto> contentDtos,
        EditPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var toCalculate = purchase.Contents
            .Join(
                contentDtos.Where(x => request.WithLogistics && x.CalculateLogistics),
                purchaseContent => purchaseContent.ProductId,
                dto => dto.ProductId,
                (purchaseContent, dto) => new PurchaseLogisticsItem(
                    purchaseContent,
                    dto.ProductId,
                    dto.Count))
            .ToList();

        await purchaseLogisticsService.ApplyAsync(
            purchase,
            toCalculate,
            request.StorageFrom,
            request.PurchaseDateTime,
            GetSystemUserId(),
            cancellationToken);
    }
}