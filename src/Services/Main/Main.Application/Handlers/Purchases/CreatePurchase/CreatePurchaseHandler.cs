using System.Data;
using Abstractions.Interfaces.Persistence;
using Abstractions.Models.Options;
using Application.Common.Extensions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Events;
using Application.Common.Interfaces.Repositories;
using Attributes;
using Contracts.Purchase;
using LinqKit;
using Main.Application.Dtos.Purchase;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Interfaces.Persistence;
using Main.Application.Interfaces.Services;
using Main.Application.Projections;
using Main.Entities.Exceptions;
using Main.Entities.Purchase;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Role = Enums.Role;

namespace Main.Application.Handlers.Purchases.CreatePurchase;

[AutoSave]
[Transactional(
    IsolationLevel.ReadCommitted,
    20,
    2)]
public record CreatePurchaseCommand(
    Guid SupplierUserId,
    Guid SupplierOrganizationId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom
) : ICommand<CreatePurchaseResult>;

public record CreatePurchaseResult(PurchaseDto Purchase);

public class CreatePurchaseHandler(
    ISender sender,
    IOptions<SystemOptions> systemOptions,
    IUserRepository userRepository,
    IPurchaseLogisticsService purchaseLogisticsService,
    IIntegrationEventScope integrationEventScope,
    IReadRepository<Purchase, Guid> readRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreatePurchaseCommand, CreatePurchaseResult>
{
    public async Task<CreatePurchaseResult> Handle(
        CreatePurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var systemId = systemOptions.Value.SystemId;

        await userRepository.EnsureExistAsync(
            request.SupplierUserId,
            _ => new UserIsNotInNeededRole(Role.Supplier),
            Criteria<User>
                .New()
                .WhereHasRole(Role.Supplier),
            cancellationToken);

        var purchaseContents = request.PurchaseContent.ToList();

        var totalSum = purchaseContents.Sum(x => x.Price * x.Count);

        var purchaseTransaction = (await sender.Send(
                new CreateTransactionCommand(
                    request.SupplierOrganizationId,
                    systemId,
                    totalSum,
                    request.CurrencyId,
                    request.PurchaseDate,
                    TransactionSourceType.Purchase,
                    TransactionCreationMode.System),
                cancellationToken))
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
                    request.SupplierOrganizationId,
                    request.PayedSum.Value,
                    request.CurrencyId,
                    request.PurchaseDate,
                    TransactionSourceType.Manual,
                    TransactionCreationMode.System),
                cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        integrationEventScope.Add(
            new PurchaseUpdateEvent
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
            request.SupplierUserId,
            request.SupplierOrganizationId,
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
                throw new InvalidOperationException(
                    "Order of storage contents and " +
                    "new purchase contents are invalid.");

            var purchaseContent = PurchaseContent.Create(
                content.ProductId,
                content.Count,
                content.Price,
                storageContent.Id,
                content.Comment);

            purchase.AddContent(purchaseContent);

            if (request.WithLogistics && content.CalculateLogistics)
                toCalculate.Add(
                    new PurchaseLogisticsItem(
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
