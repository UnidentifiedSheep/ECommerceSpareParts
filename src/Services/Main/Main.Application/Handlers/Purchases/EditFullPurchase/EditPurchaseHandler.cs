using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Logistics;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Entities.Event;
using Main.Entities.Exceptions.Products;
using Main.Entities.Exceptions.Purchase;
using Main.Entities.Exceptions.Storages;
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
    string PurchaseId,
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
    IRepository<StorageOwner, (string, Guid)> storageOwnerRepository,
    IStorageContentRepository storageContentRepository,
    IProductRepository productRepository,
    ICurrencyConverter currencyConverter,
    IUnitOfWork unitOfWork) : ICommandHandler<EditPurchaseCommand>
{
    public async Task<Unit> Handle(EditPurchaseCommand request, CancellationToken cancellationToken)
    {
        var purchaseId = ParsePurchaseId(request.PurchaseId);
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

    private static Guid ParsePurchaseId(string purchaseId)
    {
        return Guid.TryParse(purchaseId, out var parsed)
            ? parsed
            : throw new PurchaseNotFoundException(Guid.Empty);
    }

    private async Task<Guid> GetSystemUserId(CancellationToken cancellationToken)
    {
        return (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
    }

    private async Task<Purchase> GetPurchase(Guid purchaseId, CancellationToken cancellationToken)
    {
        var criteria = Criteria<Purchase>.New()
            .Where(x => x.Id == purchaseId)
            .Include(x => x.Contents)
            .Include(x => x.PurchaseLogistic)
            .Track()
            .ForUpdate()
            .Build();

        return await purchaseRepository.FirstOrDefaultAsync(criteria, cancellationToken)
               ?? throw new PurchaseNotFoundException(purchaseId);
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
            .OfType<int>()
            .ToHashSet();

        var storageContents = storageContentIds.Count == 0
            ? new Dictionary<int, StorageContent>()
            : await storageContentRepository.EnsureExistsForUpdateAsync(
                storageContentIds,
                ids => new StorageContentNotFoundException(ids[0]),
                cancellationToken);

        var storageEvents = new List<Event>();

        foreach (var removed in purchase.Contents.Where(x => !requestedIds.Contains(x.Id)).ToList())
            RemoveContent(purchase, removed, storageContents, products, storageEvents);

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

    private void RemoveContent(
        Purchase purchase,
        PurchaseContent content,
        Dictionary<int, StorageContent> storageContents,
        Dictionary<int, Product> products,
        List<Event> storageEvents)
    {
        if (content.PurchaseContentLogistic is { } logistic)
            unitOfWork.Remove(logistic);

        if (content.StorageContentId is { } storageContentId &&
            storageContents.TryGetValue(storageContentId, out var storageContent))
        {
            storageEvents.Add(StorageMovementEvent.Create(storageContent, StorageMovementType.StorageContentDeletion));
            products[storageContent.ProductId].IncreaseStock(-storageContent.Count);
            storageContent.SetCount(0);
        }

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

        if (content.StorageContentId is not { } storageContentId ||
            !storageContents.TryGetValue(storageContentId, out var storageContent))
            throw new StorageContentNotFoundException(content.StorageContentId ?? 0);

        storageEvents.Add(StorageMovementEvent.Create(storageContent, StorageMovementType.StorageContentEditing));

        products[content.ProductId].IncreaseStock(dto.Count - content.Count);

        content.SetCount(dto.Count);
        content.SetPrice(dto.Price);
        content.SetComment(dto.Comment);

        storageContent.SetCount(dto.Count);
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
                (purchaseContent, dto) => new
                {
                    PurchaseContent = purchaseContent,
                    Item = new LogisticsItemDto
                    {
                        ProductId = dto.ProductId,
                        Quantity = dto.Count
                    }
                })
            .ToList();

        if (toCalculate.Count == 0)
        {
            ClearLogistics(purchase);
            return;
        }

        if (request.StorageFrom == null)
            throw new InvalidOperationException("Storage from must be set, when calculation logistics.");

        var deliveryCost = await sender.Send(
            new CalculateDeliveryCostQuery(
                request.StorageFrom,
                purchase.Storage,
                toCalculate.Select(x => x.Item),
                LogisticsCalculationMode.Strict),
            cancellationToken);

        var route = deliveryCost.Route;
        for (var i = 0; i < toCalculate.Count; i++)
        {
            var calcResult = deliveryCost.DeliveryCost.Items[i];
            toCalculate[i].PurchaseContent.SetLogistic(
                calcResult.Weight,
                calcResult.AreaM3,
                calcResult.Cost);
        }

        foreach (var content in purchase.Contents.Except(toCalculate.Select(x => x.PurchaseContent)).ToList())
            if (content.ClearLogistic() is { } logistic)
                unitOfWork.Remove(logistic);

        Transaction? logisticsPayment = null;
        if (route.CarrierId is not null)
            logisticsPayment = await CreateTransaction(
                route.CarrierId.Value,
                await GetSystemUserId(cancellationToken),
                deliveryCost.DeliveryCost.TotalCost,
                route.Currency.Id,
                request.PurchaseDateTime,
                cancellationToken);

        purchase.SetPurchaseLogistic(
            route.Id,
            route.Currency.Id,
            route.PricingModel,
            route.RouteType,
            route.PricePerKg,
            route.PricePerM3,
            route.PricePerOrder,
            route.MinimumPrice,
            logisticsPayment?.Id,
            deliveryCost.DeliveryCost.MinimalPriceApplied);
    }

    private void ClearLogistics(Purchase purchase)
    {
        foreach (var content in purchase.Contents)
            if (content.ClearLogistic() is { } logistic)
                unitOfWork.Remove(logistic);

        if (purchase.ClearPurchaseLogistic() is { } purchaseLogistic)
            unitOfWork.Remove(purchaseLogistic);
    }
}
