using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Interfaces.Services;
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Contracts.Purchase;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Projections;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Entities.Auth;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Entities.Setting;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Role = Main.Enums.Role;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record CreatePurchaseCommand(
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(PurchaseDto Purchase);

public class CreatePurchaseHandler(
    ISender sender,
    ISettingsService settingsService,
    IUserRepository userRepository,
    IPurchaseLogisticsService purchaseLogisticsService,
    IIntegrationEventScope integrationEventScope,
    IReadRepository<Purchase, Guid> readRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<CreatePurchaseResult> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        var systemId = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;

        var supplier = await userRepository.EnsureExistAsync(
            request.SupplierId,
            _ => new UserIsNotInNeededRole(Role.Supplier),
            Criteria<User>
                .New()
                .WhereHasRole(Role.Supplier),
            cancellationToken);

        var purchaseContents = request.PurchaseContent.ToList();

        var totalSum = purchaseContents.Sum(x => x.Price * x.Count);

        var purchaseTransaction = (await sender.Send(
                new CreateTransactionCommand(
                    supplier.Id,
                    systemId,
                    totalSum,
                    request.CurrencyId,
                    request.PurchaseDate,
                    TransactionSourceType.Purchase), cancellationToken))
            .Transaction;

        var storageContents = await AddContentsToStorage(
            request,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var purchase = await CreatePurchase(
            systemId,
            request,
            purchaseTransaction.Id,
            purchaseContents,
            storageContents,
            cancellationToken);

        if (request.PayedSum > 0)
            await sender.Send(
                new CreateTransactionCommand(
                    systemId,
                    supplier.Id,
                    request.PayedSum.Value,
                    request.CurrencyId,
                    request.PurchaseDate,
                    TransactionSourceType.Manual),
                cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        integrationEventScope.Add(new PurchaseUpdateEvent
        {
            PurchaseId = purchase.Id 
        });

        var fromDb = await readRepository.Query
            .AsExpandable()
            .Select(PurchaseProjections.ToPurchaseDto)
            .FirstAsync(x => x.Id == purchase.Id, cancellationToken);
        return new CreatePurchaseResult(fromDb);
    }

    private async Task<IReadOnlyList<StorageContent>> AddContentsToStorage(
        CreatePurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var command = new AddContentCommand(
            request.PurchaseContent.Select(x => new NewStorageContentDto
            {
                BuyPrice = x.Price,
                Count = x.Count,
                CurrencyId = request.CurrencyId,
                PurchaseDate = request.PurchaseDate,
                ProductId = x.ProductId
            }),
            request.StorageName,
            StorageMovementType.Purchase);

        return (await sender.Send(command, cancellationToken))
            .StorageContents;
    }

    private async Task<Purchase> CreatePurchase(
        Guid systemId,
        CreatePurchaseCommand request,
        Guid transactionId,
        IReadOnlyList<NewPurchaseContentDto> purchaseContents,
        IReadOnlyList<StorageContent> storageContents,
        CancellationToken cancellationToken)
    {
        List<PurchaseLogisticsItem> toCalculate = [];
        var purchase = Purchase.Create(
            request.SupplierId,
            request.CurrencyId,
            transactionId,
            request.StorageName,
            request.PurchaseDate);

        purchase.SetComment(request.Comment);

        for (var i = 0; i < storageContents.Count; i++)
        {
            var content = purchaseContents[i];
            var storageContent = storageContents[i];

            if (content.ProductId != storageContent.ProductId)
                throw new InvalidOperationException("Order of storage contents and " +
                                                    "new purchase contents are invalid.");

            var purchaseContent = PurchaseContent.Create(
                content.ProductId,
                content.Count,
                content.Price,
                storageContent.Id,
                content.Comment);

            purchase.AddContent(purchaseContent);

            if (request.WithLogistics && content.CalculateLogistics)
                toCalculate.Add(new PurchaseLogisticsItem(
                    purchaseContent,
                    content.ProductId,
                    content.Count));
        }

        await purchaseLogisticsService.ApplyAsync(
            purchase,
            toCalculate,
            request.StorageFrom,
            request.PurchaseDate,
            systemId,
            cancellationToken);

        purchase.Complete();
        await unitOfWork.AddAsync(purchase, cancellationToken);
        return purchase;
    }
}