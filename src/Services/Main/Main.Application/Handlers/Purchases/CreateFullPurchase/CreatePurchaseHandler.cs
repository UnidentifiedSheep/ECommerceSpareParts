using System.Data;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Settings;
using Attributes;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Dtos.Logistics;
using Main.Application.Dtos.Storage;
using Main.Application.Extensions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Logistics.CalculateDeliveryCost;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Balance;
using Main.Entities.Exceptions.Auth;
using Main.Entities.Purchase;
using Main.Entities.Setting;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.Purchases.CreateFullPurchase;

[AutoSave]
[Transactional(IsolationLevel.ReadCommitted, 20, 2)]
public record CreatePurchaseCommand(
    Guid CreatedUserId,
    Guid SupplierId,
    int CurrencyId,
    string StorageName,
    DateTime PurchaseDate,
    IEnumerable<NewPurchaseContentDto> PurchaseContent,
    string? Comment,
    decimal? PayedSum,
    bool WithLogistics,
    string? StorageFrom) : ICommand;

public class CreatePurchaseHandler(
    ISender sender,
    ISettingsService settingsService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePurchaseCommand>
{
    public async Task<Unit> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        Guid systemId = (await settingsService.GetOrDefault<GlobalApplicationSetting>(cancellationToken))
            .Data
            .SystemId;
        
        User supplier = await userRepository.EnsureExistAsync(
            request.SupplierId,
            _ => new UserIsNotInNeededRole(Role.Supplier),
            Criteria<User>
                .New()
                .Where(x => x.Roles.Any(z => z.RoleName== nameof(Role.Supplier))),
            cancellationToken);

        var purchaseContents = request.PurchaseContent.ToList();
        
        decimal totalSum = purchaseContents.Sum(x => x.Price * x.Count);

        var purchaseTransaction = (await sender.Send(
            new CreateTransactionCommand(
                supplier.Id,
                systemId,
                totalSum,
                request.CurrencyId,
                request.PurchaseDate), cancellationToken))
            .Transaction;
        
        var storageContents = await AddContentsToStorage(
            request, 
            cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await CreatePurchase(
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
                    request.PurchaseDate),
                cancellationToken);

        return Unit.Value;
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

    private async Task CreatePurchase(
        Guid systemId,
        CreatePurchaseCommand request,
        Guid transactionId,
        IReadOnlyList<NewPurchaseContentDto> purchaseContents,
        IReadOnlyList<StorageContent> storageContents,
        CancellationToken cancellationToken)
    {
        Dictionary<PurchaseContent, LogisticsItemDto> toCalculate = [];
        var purchase = Purchase.Create(
            request.SupplierId,
            request.CurrencyId,
            transactionId,
            request.StorageName,
            request.PurchaseDate);

        purchase.SetComment(request.Comment);

        for (int i = 0; i < storageContents.Count; i++)
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
                toCalculate[purchaseContent] = new LogisticsItemDto
                {
                    ProductId = content.ProductId,
                    Quantity = content.Count
                };
        }

        if (toCalculate.Count != 0)
        {
            var deliveryCost = await CalculateDelivery(toCalculate.Values, request, cancellationToken);
            StorageRouteDto route = deliveryCost.Route;

            int index = 0;
            foreach (var (purchaseContent, _) in toCalculate)
            {
                var calcResult = deliveryCost.DeliveryCost.Items[index++];
                purchaseContent.SetLogistic(calcResult.Weight, calcResult.AreaM3, calcResult.Cost);
            }

            Transaction? logisticsPayment = null;
            if (route.CarrierId != null)
            {
                User carrier = await userRepository.EnsureExistAsync(
                    key: route.CarrierId.Value,
                    errorFactory: id => new UserNotFoundException(id),
                    ct: cancellationToken);
                
                logisticsPayment = (await sender.Send(
                        new CreateTransactionCommand(
                            carrier.Id,
                            systemId,
                            deliveryCost.DeliveryCost.TotalCost,
                            route.Currency.Id,
                            request.PurchaseDate), cancellationToken))
                    .Transaction;
                
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            
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
        
        purchase.Complete();
        await unitOfWork.AddAsync(purchase, cancellationToken);
    }

    private async Task<CalculateDeliveryCostResult> CalculateDelivery(
        IEnumerable<LogisticsItemDto> toCalculate,
        CreatePurchaseCommand request,
        CancellationToken cancellationToken)
    {
        if (request.StorageFrom == null)
            throw new InvalidOperationException("Storage from must be set, when calculation logistics.");
        
        var calculateCommand = new CalculateDeliveryCostQuery(
            request.StorageFrom,
            request.StorageName,
            toCalculate,
            LogisticsCalculationMode.Strict);
        
        return await sender.Send(calculateCommand, cancellationToken);
    }
}
