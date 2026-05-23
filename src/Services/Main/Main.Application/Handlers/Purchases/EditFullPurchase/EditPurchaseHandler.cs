using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Extensions.Repository;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Product;
using Main.Entities.Purchase;
using Main.Entities.Setting;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Event = Main.Entities.Event.Event;

namespace Main.Application.Handlers.Purchases.EditFullPurchase;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record EditPurchaseCommand(
    IEnumerable<EditPurchaseDto> Content,
    Guid PurchaseId,
    int CurrencyId,
    string? Comment,
    DateTime PurchaseDateTime,
    Guid UpdatedUserId,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class EditPurchaseHandler(
    ISender sender,
    ISettingsService settingsService,
    IRepository<Purchase, Guid> purchaseRepository,
    IRepository<PurchaseContent, int> purchaseContentRepository,
    IRepository<PurchaseLogistic, Guid> purchaseLogisticRepository,
    IRepository<StorageOwner, (string, Guid)> storageOwnerRepository,
    IStorageContentRepository storageContentRepository,
    IProductRepository productRepository,
    ICurrencyConverter currencyConverter,
    IPurchaseLogisticsService purchaseLogisticsService,
    IUnitOfWork unitOfWork) : ICommandHandler<EditPurchaseCommand>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = request.PurchaseId;
        var purchase = await GetPurchase(purchaseId, cancellationToken);
        var contentDtos = request.Content.ToList();

        await EnsureStorageOwner(purchase, request, cancellationToken);
        await ReversePurchaseTransactions(purchase, request.UpdatedUserId, cancellationToken);

        var totalSum = contentDtos.Sum(x => x.Price * x.Count);
        var purchaseTransaction = await CreateTransaction(
            purchase.SupplierId,
            await GetSystemUserId(cancellationToken),
            totalSum,
            request.CurrencyId,
            request.PurchaseDateTime,
            cancellationToken);

        purchase.SetTransactionId(purchaseTransaction.Id);
        purchase.SetCurrencyId(request.CurrencyId);
        purchase.SetPurchaseDate(request.PurchaseDateTime);
        purchase.SetComment(request.Comment);

        await UpdateContents(purchase, contentDtos, request, cancellationToken);
        await UpdateLogistics(purchase, contentDtos, request, cancellationToken);

        return Unit.Value;
    }

    private async Task<Guid> GetSystemUserId(CancellationToken cancellationToken)
    {
        return (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
    }

    private async Task<Purchase> GetPurchase(Guid purchaseId, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetPurchaseForUpdate(purchaseId, cancellationToken);

        await purchaseContentRepository.GetPurchaseContents(purchaseId, cancellationToken);
        await purchaseLogisticRepository.FirstOrDefaultAsync(
            Criteria<PurchaseLogistic>.New()
                .Where(x => x.PurchaseId == purchaseId)
                .Track()
                .Build(),
            cancellationToken);

        return purchase;
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
        Guid updatedUserId,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ReverseTransactionCommand(purchase.TransactionId, updatedUserId),
            cancellationToken);

        if (purchase.PurchaseLogistic?.TransactionId is { } logisticsTransactionId)
            await sender.Send(
                new ReverseTransactionCommand(logisticsTransactionId, updatedUserId),
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
                    transactionDateTime),
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

        var affectedProductIds = contentDtos
            .Select(x => x.ProductId)
            .Concat(purchase.Contents.Select(x => x.ProductId))
            .ToHashSet();

        var products = await productRepository.EnsureExistsForUpdateAsync(
            affectedProductIds,
            ids => new ProductNotFoundException(ids),
            cancellationToken);

        var storageContentIds = purchase.Contents
            .Select(x => x.StorageContentId)
            .ToHashSet();

        var storageContents = storageContentIds.Count == 0
            ? new Dictionary<int, StorageContent>()
            : await storageContentRepository.EnsureExistsForUpdateAsync(
                storageContentIds,
                ids => new StorageContentNotFoundException(ids[0]),
                cancellationToken);

        var storageEvents = new List<Event>();

        foreach (var removed in purchase.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
            await RemoveContent(purchase, removed, cancellationToken);

        foreach (var dto in contentDtos)
        {
            if (dto.Id is null)
            {
                await AddContent(purchase, dto, request, products, storageEvents, cancellationToken);
                continue;
            }

            await UpdateContent(
                existingById[dto.Id.Value],
                dto,
                request,
                storageContents,
                products,
                storageEvents,
                cancellationToken);
        }

        await unitOfWork.AddRangeAsync(storageEvents, cancellationToken);
    }

    private async Task RemoveContent(
        Purchase purchase,
        PurchaseContent content,
        CancellationToken cancellationToken)
    {
        if (content.PurchaseContentLogistic is { } logistic)
            unitOfWork.Remove(logistic);

        await sender.Send(
            new SubtractStorageContentsCommand(
                content.StorageContentId,
                content.Count,
                StorageMovementType.PurchaseDeletion),
            cancellationToken);

        purchase.RemoveContent(content);
        unitOfWork.Remove(content);
    }

    private async Task AddContent(
        Purchase purchase,
        EditPurchaseDto dto,
        EditPurchaseCommand request,
        IReadOnlyDictionary<int, Product> products,
        ICollection<Event> storageEvents,
        CancellationToken cancellationToken)
    {
        var baseCurrencyId = (await settingsService.GetOrDefault<CurrencySetting>(cancellationToken))
            .Data
            .BaseCurrencyId;
        var buyPriceInBaseCurrency = await currencyConverter.ConvertToBaseAsync(
            dto.Price,
            request.CurrencyId,
            cancellationToken);
        var storageContent = StorageContent.Create(
            purchase.Storage,
            dto.ProductId,
            dto.Count,
            dto.Price,
            request.CurrencyId,
            buyPriceInBaseCurrency,
            baseCurrencyId,
            request.PurchaseDateTime);

        await unitOfWork.AddAsync(storageContent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        products[dto.ProductId].IncreaseStock(dto.Count);
        storageEvents.Add(StorageMovementEvent.Create(storageContent, StorageMovementType.Purchase));

        var purchaseContent = PurchaseContent.Create(
            dto.ProductId,
            dto.Count,
            dto.Price,
            storageContent.Id,
            dto.Comment);

        purchase.AddContent(purchaseContent);
    }

    private async Task UpdateContent(
        PurchaseContent content,
        EditPurchaseDto dto,
        EditPurchaseCommand request,
        Dictionary<int, StorageContent> storageContents,
        Dictionary<int, Product> products,
        List<Event> storageEvents,
        CancellationToken cancellationToken)
    {
        if (content.ProductId != dto.ProductId)
            throw new ArticleDoesntMatchContentException(content.Id);

        if (!storageContents.TryGetValue(content.StorageContentId, out var storageContent))
            throw new StorageContentNotFoundException(content.StorageContentId);

        var countDelta = dto.Count - content.Count;
        if (countDelta < 0)
        {
            await sender.Send(
                new SubtractStorageContentsCommand(
                    content.StorageContentId,
                    -countDelta,
                    StorageMovementType.PurchaseEditing),
                cancellationToken);
        }
        else if (countDelta > 0)
        {
            storageEvents.Add(StorageMovementEvent.Create(storageContent, StorageMovementType.PurchaseEditing));
            products[content.ProductId].IncreaseStock(countDelta);
            storageContent.IncreaseCount(countDelta);
        }

        content.SetCount(dto.Count);
        content.SetPrice(dto.Price);
        content.SetComment(dto.Comment);

        storageContent.SetCurrencyId(request.CurrencyId);
        storageContent.SetPurchaseDate(request.PurchaseDateTime);
        storageContent.SetBuyPrice(
            dto.Price,
            await currencyConverter.ConvertToBaseAsync(dto.Price, request.CurrencyId, cancellationToken));
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
            await GetSystemUserId(cancellationToken),
            cancellationToken);
    }
}